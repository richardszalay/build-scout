using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using Microsoft.Silverlight.Testing.Harness;
using System.Reflection;
using System.Collections.Generic;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RichardSzalay.PocketCiTray.Tests.Infrastructure
{
    public class BddTestAssembly : IAssembly
    {
        /// <summary>
        /// Assembly reflection object.
        /// </summary>
        private Assembly _assembly;

        /// <summary>
        /// Assembly initialization method information.
        /// </summary>
        private LazyMethodInfo _init;

        /// <summary>
        /// Assembly cleanup method information.
        /// </summary>
        private LazyMethodInfo _cleanup;

        /// <summary>
        /// Unit test provider used for the assembly.
        /// </summary>
        private IUnitTestProvider _provider;

        /// <summary>
        /// The unit test harness.
        /// </summary>
        private UnitTestHarness _harness;

        /// <summary>
        /// Creates a new unit test assembly wrapper.
        /// </summary>
        /// <param name="provider">Unit test metadata provider.</param>
        /// <param name="unitTestHarness">A reference to the unit test harness.</param>
        /// <param name="assembly">Assembly reflection object.</param>
        public BddTestAssembly(IUnitTestProvider provider, UnitTestHarness unitTestHarness, Assembly assembly)
        {
            _provider = provider;
            _harness = unitTestHarness;
            _assembly = assembly;
            _init = new LazyAssemblyMethodInfo(_assembly, typeof(AssemblyInitializeAttribute));
            _cleanup = new LazyAssemblyMethodInfo(_assembly, typeof(AssemblyCleanupAttribute));
        }

        /// <summary>
        /// Gets the name of the test assembly.
        /// </summary>
        public string Name
        {
            get
            {
                string n = _assembly.ToString();
                return (n.Contains(", ") ? n.Substring(0, n.IndexOf(",", StringComparison.Ordinal)) : n);
            }
        }

        /// <summary>
        /// Gets the unit test provider instance.
        /// </summary>
        public IUnitTestProvider Provider
        {
            get { return _provider; }
        }

        /// <summary>
        /// Gets any assembly initialize method.
        /// </summary>
        public MethodInfo AssemblyInitializeMethod
        {
            get { return _init.GetMethodInfo(); }
        }

        /// <summary>
        /// Gets any assembly cleanup method.
        /// </summary>
        public MethodInfo AssemblyCleanupMethod
        {
            get { return _cleanup.GetMethodInfo(); }
        }

        /// <summary>
        /// Gets the test harness used to initialize the assembly.
        /// </summary>
        public UnitTestHarness TestHarness
        {
            get { return _harness; }
        }

        /// <summary>
        /// Gets the test harness as a unit test harness.
        /// </summary>
        public UnitTestHarness UnitTestHarness
        {
            get { return _harness as UnitTestHarness; }
        }

        /// <summary>
        /// Reflect and retrieve the test class metadata wrappers for 
        /// the test assembly.
        /// </summary>
        /// <returns>Returns a collection of test class metadata 
        /// interface objects.</returns>
        public ICollection<ITestClass> GetTestClasses()
        {

            ICollection<Type> classes = ReflectionUtility.GetTypesWithAttribute(_assembly, typeof(TestClassAttribute));
            List<ITestClass> tests = new List<ITestClass>(classes.Count);
            foreach (Type type in classes)
            {
                tests.Add(new BddTestClass(this, type));
            }
            return tests;
        }
    }
}
