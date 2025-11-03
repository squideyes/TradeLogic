using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TradeLogic.UnitTests.TestFramework
{
    public class TestRunner
    {
        public static void Main(string[] args)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var testTypes = assembly.GetTypes()
                .Where(t => t.Namespace == "TradeLogic.UnitTests" && t.Name.EndsWith("Tests"))
                .ToList();

            int totalTests = 0;
            int passedTests = 0;
            int failedTests = 0;
            var failures = new List<string>();

            foreach (var testType in testTypes)
            {
                Console.WriteLine($"\n{testType.Name}:");
                var testMethods = testType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.GetCustomAttribute<TestAttribute>() != null)
                    .ToList();

                var instance = Activator.CreateInstance(testType);

                foreach (var method in testMethods)
                {
                    totalTests++;
                    try
                    {
                        method.Invoke(instance, null);
                        Console.WriteLine($"  ✓ {method.Name}");
                        passedTests++;
                    }
                    catch (TargetInvocationException ex)
                    {
                        Console.WriteLine($"  ✗ {method.Name}");
                        failedTests++;
                        failures.Add($"{testType.Name}.{method.Name}: {ex.InnerException?.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  ✗ {method.Name}");
                        failedTests++;
                        failures.Add($"{testType.Name}.{method.Name}: {ex.Message}");
                    }
                }
            }

            Console.WriteLine($"\n\n{'='} Test Results {'='}");
            Console.WriteLine($"Total: {totalTests}, Passed: {passedTests}, Failed: {failedTests}");

            if (failures.Count > 0)
            {
                Console.WriteLine($"\nFailures:");
                foreach (var failure in failures)
                {
                    Console.WriteLine($"  - {failure}");
                }
                Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("\nAll tests passed!");
                Environment.Exit(0);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class TestAttribute : Attribute
    {
    }
}

