# TradeLogic — Implementation Plan (for .NET Framework 4.8.1)

**Purpose:** Manage the full trade intent + lifecycle for a **single Position** (entries are FOK; exits via SL/TP/EoS/Manual). Venue-agnostic, event-driven integration (e.g., NinjaTrader 8 adapter).

---

## 0) Goals

- **Single Position** lifecycle per `PositionManager` (no multi-position netting).
- **Entries:** Market, Limit, Stop, StopLimit — **all FOK** (no residual/partials allowed).
- **Exits:** StopLoss (SL), TakeProfit (TP), EndOfSession (GTD), or manual `GoFlat()`.
- Internal **Fill** tracking and immutable **Trade** emission at close.
- **Host-driven** venue integration via events and callbacks (no venue SDK references).
- Deterministic, thread-safe, unit-testable. Targets **.NET Framework 4.8.1** (C# 7.3).

---

## 1) Project layout (suggested split)

```
TradeLogic/
  Contracts/
    Enums.cs                  // Side, OrderType, TimeInForce, OrderStatus, PositionState, ExitReason, ExitAtSessionEndMode
    ValueObjects.cs           // Price/Quantity wrappers (optional), identifiers
    DTOs.cs                   // OrderSpec, OrderSnapshot, Fill, PositionView, Trade, OrderUpdate
  Config/
    SessionConfig.cs
    PositionConfig.cs
  Abstractions/
    IClock.cs, IFeeModel.cs, IIdGenerator.cs, ILogger.cs
  Defaults/
    SystemClock.cs, GuidIdGenerator.cs, NoopLogger.cs, FlatFeeModel.cs
  Events/
    Delegates.cs, EventArgs.cs
  Engine/
    PositionManager.cs        // state machine + orchestration
  Diagnostics/
    (optional) Telemetry hooks, audit/event-sourcing helpers
  Persistence/
    (optional) JSON serializers for DTOs
```

> **Note:** Public surface area (namespaces and file split) is for clarity only; runtime behavior equals the single-file version you compiled.

---

## 2) Core domain model

### Enums
- `Side` = { Long, Short }
- `OrderType` = { Market, Limit, Stop, StopLimit }
- `TimeInForce` = { FOK, IOC, DAY, GTD }
- `OrderStatus` = { New, Accepted, Rejected, Canceled, Working, Filled, PartiallyFilled, Expired }
- `PositionState` = { Flat, PendingEntry, Open, PendingExit, Closing, Closed, Error }
- `ExitReason` = { TakeProfit, StopLoss, EndOfSession, Manual }
- `ExitAtSessionEndMode` = { GTDMarket, GTDMarketableLimit, CancelAndMarket }

### DTOs (immutable or snapshot)
- **OrderSpec**: client + venue IDs, side, type, qty, TIF, prices (limit/stop), GTT, `IsEntry/IsExit`, `OcoGroupId`.
- **OrderSnapshot**: `Spec` + status, filled qty, avg fill price, reason.
- **Fill**: order id (client or venue), fill id, price, qty, commission, utc time.
- **PositionView**: state, side, net qty, avg entry, realized/unrealized, open/close utc, symbol, SL/TP snapshot.
- **Trade** (immutable): ids, side, open/close utc, exit reason, net qty, avg entry/exit, realized PnL, total fees, slippage, entry/exit fills.
- **OrderUpdate**: venue → library status bridge (clientId, venueId, status, reason).

### Config
- **SessionConfig**: `TimeZoneId`, `SessionStartLocal`, `SessionEndLocal`, helpers to compute today’s start/end in **UTC** with cross-OS TZ fallback (`America/New_York` → `Eastern Standard Time`).
- **PositionConfig**: symbol, tick size, point value, min qty, session, id prefix, EOS mode, marketable-limit offset ticks, SL as Stop vs StopLimit, manual flatten as Market vs marketable Limit, slippage tolerance (ticks).

### Abstractions
- `IClock`, `IFeeModel`, `IIdGenerator`, `ILogger` (no external deps). Defaults provided.

---

## 3) Public API (high level)

```csharp
// Construction
PositionManager(PositionConfig cfg, IClock clock, IFeeModel fees, IIdGenerator ids, ILogger log);

// Commands (host calls into library)
string SubmitEntry(OrderType type, Side side, int qty, decimal? limit=null, decimal? stop=null); // always FOK
void   ArmExits(decimal? stopLossPrice, decimal? takeProfitPrice);
void   ReplaceExits(decimal? newSL, decimal? newTP);
void   GoFlat();                                       // cancel exits + immediate exit (market/marketable-limit)
void   ConfigureEndOfSession(ExitAtSessionEndMode m);  // runtime policy change
void   OnClock(DateTime utcNow);                       // host-driven time tick for EoS handling
void   Reset();                                        // after Closed (or Flat) to reuse manager

// Venue callbacks (host forwards broker/exchange updates)
void   OnOrderAccepted(OrderUpdate u);
void   OnOrderRejected(OrderUpdate u);
void   OnOrderCanceled(OrderUpdate u);
void   OnOrderExpired(OrderUpdate u);
void   OnOrderPartiallyFilled(string clientOrderId, string fillId, decimal price, int qty, DateTime fillUtc);
void   OnOrderFilled(string clientOrderId, string fillId, decimal price, int qty, DateTime fillUtc);

// Read-only
PositionView GetView();
Guid PositionId { get; }
```

### Events (library → host)
- Order lifecycle: `OrderSubmitted`, `OrderAccepted`, `OrderRejected`, `OrderCanceled`, `OrderExpired`, `OrderWorking`, `OrderPartiallyFilled`, `OrderFilled`.
- Position lifecycle: `PositionOpened`, `ExitArmed`, `ExitReplaced`, `PositionUpdated`, `PositionClosing`, `PositionClosed`.
- Trade emission: `TradeFinalized` (immutable `Trade`).
- Errors/warnings: `ErrorOccurred(code, message, context)` (includes **FOK_PARTIAL**, **EXIT_REJECTED**, **CANCEL_REQUEST**, **SLIPPAGE_WARN**).

---

## 4) State machine (single-position)

**States:** `Flat → PendingEntry → Open → PendingExit/Closing → Closed` (+ `Error` safety).

**Transitions (selected):**
- `Flat` → `PendingEntry` on `SubmitEntry(FOK)`.
- Entry **Accepted**/**Working**: remain `PendingEntry`.
- Entry **Filled** (full) → `Open` (auto-place armed OCO exits).
- Entry **Rejected/Canceled/Expired** → `Flat`.
- Entry **PartiallyFilled** (shouldn’t happen under FOK): raise `FOK_PARTIAL`, request cancel, revert to `Flat` when cancel confirmed.
- `Open` + exit fills → `PendingExit` (until qty→0).
- When qty→0 → `Closed` → emit `Trade`; sibling OCO leg canceled.
- `OnClock` hitting session end triggers EoS policy → `Closing` then `Closed`.

---

## 5) FOK semantics (entries)

- Entries **always** created with `TimeInForce=FOK` in `OrderSpec`.
- If venue supports FOK natively: expect either **Filled (full)** or **Rejected/Expired/Canceled**.
- If a **partial** fill leaks in:
  - Library raises `FOK_PARTIAL` + `OrderPartiallyFilled` and expects host to cancel remainder.
  - After cancel confirmation, manager returns to `Flat` (no lingering open qty).

---

## 6) Exit orchestration (OCO)

- On transition to **Open**, if exits are **armed**, library submits OCO pair:
  - **SL**: `Stop` (or `StopLimit` if configured) @ armed SL; `GTD` with `GoodTillTimeUtc` = session end.
  - **TP**: `Limit` @ armed TP; same `GTD` and `OcoGroupId`.
- `ReplaceExits(newSL, newTP)` cancels old legs, then places the new OCO atomically.
- If one leg fills while replacing, the other is auto-canceled; manager keeps consistency.

**End-of-Session (EoS) modes:**
- `GTDMarket`: keep GTD exits; if none exist, submit immediate exit (market).
- `GTDMarketableLimit`: if none exist, submit marketable **Limit** (offset ticks).
- `CancelAndMarket`: cancel any working leg(s), then submit **Market** exit.
- In all cases, manager transitions to **Closing**, then **Closed**, emits `Trade(ExitReason=EndOfSession)`.

**Manual flatten (`GoFlat`)**:
- Cancel working exits; submit immediate exit (`Market` or marketable `Limit` per config).
- Transition to **Closing**; upon full fill: **Closed** (ExitReason=Manual).

---

## 7) PnL, fees, slippage

- **Realized PnL** on exit fills: 
  - Long: `(exitPrice - avgEntry) * pointValue * closedQty - fees`
  - Short: `(avgEntry - exitPrice) * pointValue * closedQty - fees`
- **Fees** via `IFeeModel` (default `FlatFeeModel(perContractPerSide)`).
- **Slippage** (informational): `|avgEntry - intendedEntry| * entryQty * pointValue`.
  - Intended entry = provided Limit/Stop/StopLimit reference; for Market entries, the first fill price.
  - If slippage ticks > `SlippageToleranceTicks`: raise `SLIPPAGE_WARN`.

---

## 8) Time & sessions

- Internally use **UTC** (`IClock.UtcNow`). `SessionConfig` converts session **local** start/end to **UTC** (Windows + Linux TZ IDs).
- Host may drive time by calling `OnClock(utcNow)` (recommended each bar or second). No timers inside the library.

---

## 9) Concurrency model

- Single-threaded **actor-style** sequencing via a private lock inside `PositionManager`.
- All public methods + callbacks acquire the same lock; events are raised **after** state mutation.
- Idempotency: snapshots keyed by `ClientOrderId`; duplicate/conflicting updates are tolerated with last-writer-wins semantics and warnings via `ErrorOccurred`.

---

## 10) Validation & invariants

- Only one active position per `PositionManager`.
- `SubmitEntry` only from `Flat`.
- Entry price requirements:
  - Limit → limit price required
  - Stop → stop price required
  - StopLimit → both required
- Quantity ≥ `MinQty`.
- SL/TP sanity (checked by host or pre-placement policies; library is permissive for venue-driven fills).
- Never leave `Open` with `openQty != requested` on FOK entries.

---

## 11) Integration (NinjaTrader 8 adapter example)

**Wire-up (high level):**
1. Construct `PositionManager` per strategy instance.
2. Subscribe to:
   - `OrderSubmitted` → place at venue; capture `VenueOrderId` if provided.
   - Other order events → reflect in UI/logs; propagate venue updates back into PM.
   - `PositionOpened/Updated/Closing/Closed`, `TradeFinalized` → strategy bookkeeping.
3. Forward venue events:
   - `OnOrderAccepted/Rejected/Canceled/Expired`
   - `OnOrderPartiallyFilled/OnOrderFilled`
4. Call `OnClock(DateTime.UtcNow)` periodically to handle EoS windows.

> The library never references NinjaTrader namespaces; your adapter layer bridges those worlds.

---

## 12) Test strategy (xUnit + FluentAssertions suggested)

- **Happy paths:**
  - FOK Limit/Stop/StopLimit/Market entries (Long & Short) → SL hit, TP hit.
- **Edge cases:**
  - FOK **partial** → cancel + revert to `Flat`.
  - ReplaceExits while one leg fills (race).
  - EoS in all three modes with/without armed exits.
  - Manual `GoFlat()` during Open and during PendingExit.
- **Math checks:**
  - Realized PnL for Long/Short with various pointValue/tickSize.
  - Fee accrual (per-contract per-side).
  - Slippage warnings when tolerance exceeded.
- **Replay tests:** event-stream replays against deterministic `IClock`.

---

## 13) Deliverables & milestones

- ✅ **PositionManager** with state machine and guards (provided in single-file reference).
- ✅ DTOs, configs, abstractions, defaults (provided).
- 🔜 Split into files & namespaces per layout above.
- 🔜 Add unit tests and replay fixtures.
- 🔜 Provide sample adapter code for NinjaTrader 8 and minimal console demo.
- 🔜 Optional: JSON serializers + event-sourced audit log.

---

## 14) Message flow (text diagram)

```
Host(Strategy)                PositionManager                   Venue/Broker
--------------               ------------------                 -------------
SubmitEntry()  ─────────────▶ creates OrderSpec(FOK) ── OrderSubmitted ──────▶ PLACE ORDER
                                   │
<────────────── OrderAccepted/Rejected/Filled/PartiallyFilled/Canceled ◀───── venue updates → map to OnOrder*

Entry Filled (full) → state=Open → if exits armed → submit OCO (SL/TP) → OrderSubmitted → PLACE BOTH
Exit fill(s) reduce qty → when qty==0 → state=Closed → emit TradeFinalized
OnClock(session end) may trigger EoS policy → flatten per mode
```

---

## 15) Example sequences

**A) FOK Limit Long → TP exit**
1. `SubmitEntry(Limit, Long, 2 @ 4500.00)` → `OrderSubmitted`.
2. Venue: `Accepted` → `OnOrderAccepted(...)`.
3. Venue: `Filled 2 @ 4500.00` → `OnOrderFilled(...)` → **Open**, exits placed (if armed).
4. Venue: `TP Filled 2 @ 4510.00` → **Closed**, `TradeFinalized(ExitReason=TakeProfit)`.

**B) FOK StopLimit Short → partial leak**
1. Submit FOK; venue: `PartiallyFilled 1/2` → `OnOrderPartiallyFilled(...)`.
2. PM raises `FOK_PARTIAL`; host cancels remainder → `OnOrderCanceled(...)`.
3. PM reverts to **Flat**; no position carried.

**C) End-of-Session flatten**
1. Position Open; no exit fills.
2. `OnClock` reaches session end → policy triggers immediate flatten.
3. On final fill → **Closed**, `TradeFinalized(ExitReason=EndOfSession)`.

---

## 16) Assumptions & constraints

- Venue supports OCO semantics (or adapter emulates them atomically).
- Host ensures `OnOrder*` callbacks are sequenced per order (no reordering).
- Library assumes price rounding to `TickSize`; adapter may further normalize.
- No timers/threads spawned inside library; host drives time and threading concerns.
- One `PositionManager` per independently-managed position (scale in/out via qty on exits is supported).

---

## 17) Next steps (Augment Code)

- Split the single file into the **layout in §1**.
- Add a **NinjaTrader 8 adapter**:
  - Map `OrderSubmitted` → Atm/managed orders; keep a map `ClientOrderId → VenueOrderId`.
  - Forward all venue callbacks to PM.
- Implement **unit tests** and a minimal **console harness** to simulate event flows.
- Optionally add **JSON audit** of Order/Fill/Trade for post-mortems.
