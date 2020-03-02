namespace OpenQA.Selenium.Appium
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using OpenQA.Selenium.Remote;

    /// <summary>
    /// Appium driver options
    /// </summary>
    public class AppiumOptions : DriverOptions
    {
        private readonly Dictionary<string, object> appiumOptions = new Dictionary<string, object>();

        /// <summary>
        /// Provides a means to add additional capabilities not yet added as type safe options
        /// for the Appium driver.
        /// </summary>
        /// <param name="optionName">The name of the capability to add.</param>
        /// <param name="optionValue">The value of the capability to add.</param>
        /// <exception cref="ArgumentException">
        /// thrown when attempting to add a capability for which there is already a type safe option, or
        /// when <paramref name="optionName"/> is <see langword="null"/> or the empty string.
        /// </exception>
        /// <remarks>Calling <see cref="AddAdditionalAppiumOption"/>
        /// where <paramref name="optionName"/> has already been added will overwrite the
        /// existing value with the new value in <paramref name="optionValue"/>.
        /// Calling this method adds capabilities to the Appium-specific options object passed to
        /// webdriver executable.</remarks>
        public void AddAdditionalAppiumOption(string optionName, object optionValue)
        {
            this.ValidateCapabilityName(optionName);
            this.appiumOptions[optionName] = optionValue;
        }

        /// <summary>
        /// Provides a means to add additional capabilities not yet added as type safe options
        /// for the Appium driver.
        /// </summary>
        /// <param name="capabilityName">The name of the capability to add.</param>
        /// <param name="capabilityValue">The value of the capability to add.</param>
        /// <exception cref="ArgumentException">
        /// thrown when attempting to add a capability for which there is already a type safe option, or
        /// when <paramref name="capabilityName"/> is <see langword="null"/> or the empty string.
        /// </exception>
        /// <remarks>Calling <see cref="AddAdditionalCapability(string, object)"/>
        /// where <paramref name="capabilityName"/> has already been added will overwrite the
        /// existing value with the new value in <paramref name="capabilityValue"/>.</remarks>
        [Obsolete("Use the AddAdditionalAppiumOption method for adding additional options")]
        public override void AddAdditionalCapability(string capabilityName, object capabilityValue)
        {
            this.AddAdditionalCapability(capabilityName, capabilityValue, false);
        }

        /// <summary>
        /// Provides a means to add additional capabilities not yet added as type safe options
        /// for the Appium driver.
        /// </summary>
        /// <param name="capabilityName">The name of the capability to add.</param>
        /// <param name="capabilityValue">The value of the capability to add.</param>
        /// <param name="isGlobalCapability">Indicates whether the capability is to be set as a global
        /// capability for the driver instead of a appium-specific option.</param>
        /// <exception cref="ArgumentException">
        /// thrown when attempting to add a capability for which there is already a type safe option, or
        /// when <paramref name="capabilityName"/> is <see langword="null"/> or the empty string.
        /// </exception>
        /// <remarks>Calling <see cref="AddAdditionalCapability(string, object, bool)"/>
        /// where <paramref name="capabilityName"/> has already been added will overwrite the
        /// existing value with the new value in <paramref name="capabilityValue"/></remarks>
        [Obsolete("Use the AddAdditionalAppiumOption method for adding additional options")]
        public void AddAdditionalCapability(string capabilityName, object capabilityValue, bool isGlobalCapability)
        {
            if (isGlobalCapability)
            {
                this.AddAdditionalOption(capabilityName, capabilityValue);
            }
            else
            {
                this.AddAdditionalAppiumOption(capabilityName, capabilityValue);
            }
        }

        /// <summary>
        /// Turn the capabilities into an desired capability
        /// </summary>
        /// <returns>A desired capability</returns>
        public override ICapabilities ToCapabilities()
        {
            SetNonCompliantKnownCapabilities(appiumOptions.Keys.Where(x => !x.Contains(":")).ToList());

            var options = this.GenerateDesiredCapabilities(false);

            // get capabilities field
            var isFieldNull = options
                .GetType()
                .GetField("capabilities", BindingFlags.NonPublic | BindingFlags.Instance) == null;

            Dictionary<string, object> cap = null;
            if (isFieldNull)
            {
                cap = (Dictionary<string, object>)options
                    .GetType()
                    .BaseType
                    .GetField("capabilities", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(options);
            }
            else
            {
                cap = (Dictionary<string, object>)options
                    .GetType()
                    .GetField("capabilities", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(options);
            }

            // exit conditions
            if (cap == null)
            {
                return options;
            }

            // add capabilities
            foreach (var item in appiumOptions)
            {
                cap[item.Key] = item.Value;
            }

            return options;
        }

        public Dictionary<string, object> ToDictionary()
        {
            return appiumOptions;
        }

        /// <summary>
        /// Add custom capabilities as Known specification compliant capabilities.
        /// This "disables" the removal of non spec compliant capabilities.
        /// </summary>
        /// <param name="caps"></param>
        private void SetNonCompliantKnownCapabilities(List<string> caps)
        {
            FieldInfo field = typeof(CapabilityType).GetField("KnownSpecCompliantCapabilityNames",
                BindingFlags.Static |
                BindingFlags.NonPublic);

            if (field == null)
            {
                return;
            }

            List<string> updatedKnownCapabilities = (List<string>)field.GetValue(null);

            updatedKnownCapabilities.AddRange(caps);

            updatedKnownCapabilities = updatedKnownCapabilities.Distinct().ToList();

            field.SetValue(null, updatedKnownCapabilities);
        }
    }
}