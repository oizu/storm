Overview
========
Dotnet.Strom.Adapter is a .NET Core 2.0 implementation of Storm multi-lang protocol. You can use it to implement CSharp components for your topology. 

Prerequisites
========
 
* .NET Core framework 2.0 and above
* Git

Install form NuGet
========
		PM> Install-Package Dotnet.Storm.Adapter

Build locally
========
Run next command

		cd /strom-mulilang/dotnet
		build.sh adapter

Creating NuGet package
========

		cd /strom-mulilang/dotnet
		build.sh nuget

Run example
========

		cd /strom-mulilang/dotnet
		run.sh

Command line parameters
========

* -c (class name) - component class to instantiate
* -a (assembly name) - dll, containing component class
* -p (parameters) - parameters will be available through Arguments property
* -l (log level) - one of TRACE, DEBUG, INFO, WARN, ERROR

The example of usage is 

		spouts:
		 - id: emit-sentence
		   className: org.apache.storm.flux.wrappers.spouts.FluxShellSpout
		   constructorArgs:
		     - ["dotnet", "Dotnet.Storm.Adapter.dll", "-c", "Dotnet.Storm.Example.EmitSentense", "-a", "Dotnet.Storm.Example", "-l", "debug"]
		     - [sentence]
		   parallelism: 1

API
========

## Common
- Properties

        protected readonly static ILog Logger;

        protected string[] Arguments;

        protected static IDictionary<string, object> Configuration;

        protected static StormContext Context;

        protected static bool IsGuarantee;

        protected static int MessageTimeout;
            
- Events

        protected event EventHandler<TaskIds> OnTaskIds;

        protected event EventHandler OnInitialized;

- Storm methods

        public void Sync()

        public void Error(string message)

        public void Metrics(string name, object value)

        public VerificationResult VerifyInput(string component, string stream, List<object> tuple)

        public VerificationResult VerifyOutput(string stream, List<object> tuple)

## Spout specific
- Methods

        protected abstract void Next();

- Events

        protected event EventHandler OnActivate;

        protected event EventHandler OnDeactivate;

- Properties

        protected bool IsEnabled = false;

- Storm methods

        public void Emit(List<object> tuple, string stream = "default", long task = 0, bool needTaskIds = false)

## Bolt specific
- Methods

         protected abstract void Execute(StormTuple tuple);

## Bolt specific
- Events

        protected event EventHandler<EventArgs> OnTick;

- Storm methods

        public void Ack(string id);

        public void Fail(string id);

        public void Emit(List<object> tuple, string stream = "default", long task = 0, List<string> anchors = null, bool needTaskIds = false);

- Methods

         protected abstract void Execute(StormTuple tuple);

# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
