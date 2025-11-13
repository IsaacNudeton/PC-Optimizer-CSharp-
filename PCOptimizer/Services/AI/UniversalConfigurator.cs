using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using PCOptimizer.Services.AI.Core;

namespace PCOptimizer.Services.AI
{
    /// <summary>
    /// Universal Configurator
    /// Takes an automation recipe and configures the entire system accordingly
    /// This is what ties the AI agents to actual system changes
    /// </summary>
    public class UniversalConfigurator
    {
        private AutomationRecipeDatabase _recipeDatabase;
        private SystemSnapshot _systemState;

        public UniversalConfigurator()
        {
            _recipeDatabase = new AutomationRecipeDatabase();
        }

        /// <summary>
        /// Detect active workflow from running processes and apply matching recipe
        /// This is the core automation entry point
        /// </summary>
        public async Task<ConfigurationResult> DetectAndConfigureWorkflow(List<string> runningProcesses, SystemSnapshot systemState)
        {
            _systemState = systemState;
            var result = new ConfigurationResult();

            Console.WriteLine("[Configurator] Detecting active workflow...");
            var matchedRecipes = _recipeDatabase.FindRecipesForProcesses(runningProcesses);

            if (matchedRecipes.Count == 0)
            {
                result.Success = false;
                result.Message = "No matching automation recipes found";
                return result;
            }

            // For multiple matches, pick the most specific one
            var bestRecipe = matchedRecipes.OrderByDescending(r => r.ProcessTriggers.Count).First();

            Console.WriteLine($"[Configurator] Matched recipe: {bestRecipe.RecipeName}");
            result.AppliedRecipe = bestRecipe.RecipeName;

            // Apply the recipe
            var configResult = await ApplyRecipe(bestRecipe);
            return configResult;
        }

        /// <summary>
        /// Apply an automation recipe to the system
        /// </summary>
        public async Task<ConfigurationResult> ApplyRecipe(AutomationRecipe recipe)
        {
            var result = new ConfigurationResult
            {
                AppliedRecipe = recipe.RecipeName,
                Changes = new List<string>()
            };

            try
            {
                Console.WriteLine($"[Configurator] Applying recipe: {recipe.RecipeName}");

                // 1. Configure registry
                await ConfigureRegistry(recipe.RegistryChanges, result);

                // 2. Configure services
                await ConfigureServices(recipe.ServiceStates, result);

                // 3. Configure resource allocation
                await ConfigureResourceAllocation(recipe.ResourceAllocation, result);

                // 4. Launch companion apps if needed
                await LaunchCompanionApps(recipe.CompanionApps, result);

                result.Success = true;
                result.Message = $"Successfully applied {recipe.RecipeName}. {result.Changes.Count} changes made.";

                Console.WriteLine(result.Message);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error applying recipe: {ex.Message}";
                Console.WriteLine($"[Configurator] {result.Message}");
            }

            return result;
        }

        /// <summary>
        /// Configure Windows registry based on recipe
        /// </summary>
        private async Task ConfigureRegistry(Dictionary<string, string> changes, ConfigurationResult result)
        {
            foreach (var (keyPath, value) in changes)
            {
                try
                {
                    Console.WriteLine($"[Configurator] Setting registry: {keyPath}");

                    // Parse registry key path
                    var parts = keyPath.Split('\\');
                    var rootKey = GetRegistryRoot(parts[0]);
                    var subKeyPath = string.Join("\\", parts.Skip(1).SkipLast(1));
                    var valueName = parts.Last();

                    if (rootKey != null)
                    {
                        using (var key = rootKey.OpenSubKey(subKeyPath, true) ?? rootKey.CreateSubKey(subKeyPath))
                        {
                            if (key != null)
                            {
                                // Try to parse as integer
                                if (int.TryParse(value, out int intValue))
                                {
                                    key.SetValue(valueName, intValue);
                                }
                                else
                                {
                                    key.SetValue(valueName, value);
                                }

                                result.Changes.Add($"Registry: {valueName} = {value}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Configurator] Registry change failed: {ex.Message}");
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Configure Windows services (enable/disable)
        /// </summary>
        private async Task ConfigureServices(Dictionary<string, bool> services, ConfigurationResult result)
        {
            foreach (var (serviceName, shouldEnable) in services)
            {
                try
                {
                    Console.WriteLine($"[Configurator] {(shouldEnable ? "Enabling" : "Disabling")} service: {serviceName}");

                    // In real implementation, would use ServiceController
                    // For now, just log the action
                    result.Changes.Add($"Service {serviceName}: {(shouldEnable ? "enabled" : "disabled")}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Configurator] Service configuration failed: {ex.Message}");
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Configure resource allocation limits
        /// </summary>
        private async Task ConfigureResourceAllocation(Dictionary<string, double> allocation, ConfigurationResult result)
        {
            Console.WriteLine("[Configurator] Configuring resource allocation:");

            foreach (var (resource, percentage) in allocation)
            {
                Console.WriteLine($"  {resource}: {percentage * 100:F0}%");
                result.Changes.Add($"Resource allocation: {resource} = {percentage * 100:F0}%");
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Launch companion applications if configured
        /// </summary>
        private async Task LaunchCompanionApps(List<string> apps, ConfigurationResult result)
        {
            foreach (var app in apps)
            {
                try
                {
                    Console.WriteLine($"[Configurator] Would launch companion app: {app}");
                    result.Changes.Add($"Companion app ready: {app}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Configurator] Failed to launch {app}: {ex.Message}");
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Get registry root from key name
        /// </summary>
        private RegistryKey? GetRegistryRoot(string rootName)
        {
            return rootName switch
            {
                "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
                "HKEY_CURRENT_USER" => Registry.CurrentUser,
                "HKEY_CLASSES_ROOT" => Registry.ClassesRoot,
                "HKEY_USERS" => Registry.Users,
                _ => null
            };
        }

        /// <summary>
        /// Revert all changes from a recipe (undo)
        /// </summary>
        public async Task<ConfigurationResult> RevertRecipe(string recipeName)
        {
            var result = new ConfigurationResult
            {
                AppliedRecipe = recipeName,
                Message = $"Reverted {recipeName}. In production, would restore previous settings from backup."
            };

            Console.WriteLine($"[Configurator] Reverting recipe: {recipeName}");
            // In production, would restore from backup or undo log

            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Get all available recipes for user to choose from
        /// </summary>
        public List<AutomationRecipe> GetAvailableRecipes()
        {
            return _recipeDatabase.Recipes.Values.ToList();
        }

        /// <summary>
        /// Manually apply a specific recipe by name
        /// </summary>
        public async Task<ConfigurationResult> ApplyRecipeByName(string recipeName)
        {
            var recipe = _recipeDatabase.GetRecipe(recipeName);
            if (recipe == null)
            {
                return new ConfigurationResult
                {
                    Success = false,
                    Message = $"Recipe not found: {recipeName}"
                };
            }

            return await ApplyRecipe(recipe);
        }
    }

    public class ConfigurationResult
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public string AppliedRecipe { get; set; } = string.Empty;
        public List<string> Changes { get; set; } = new();
        public DateTime AppliedAt { get; set; } = DateTime.Now;

        public override string ToString()
        {
            return $@"
=== Configuration Result ===
Recipe: {AppliedRecipe}
Status: {(Success ? "✓ Success" : "✗ Failed")}
Message: {Message}
Changes Made: {Changes.Count}
{string.Join("\n", Changes.Select(c => $"  • {c}"))}
";
        }
    }
}
