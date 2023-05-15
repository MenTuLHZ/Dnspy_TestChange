using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using dnlib.DotNet;
using dnlib.PE;
using dnSpy.Contracts.Debugger;
using dnSpy.Contracts.Debugger.Breakpoints.Code;
using dnSpy.Contracts.Debugger.Code;
using dnSpy.Contracts.Debugger.DotNet.Code;
using dnSpy.Contracts.Documents;
using dnSpy.Contracts.Metadata;
using dnSpy.Debugger.DbgUI;

namespace dnSpy.Debugger.Test_Print_Log {

	[Flags]
	enum ESpecial_Check_Step {
		IsSpecialMethod = 1,
		IsSampleModule = 2,
		IsMemoryModule = 4,
		NothingSpecial = 8,
	}
	public class Test_Print_Log {

		public static ModuleDef? ModuleDef_Into;
		public static ModuleId? MemModuleId;
		public static bool isMemoryModule = false;
		private static AutoResetEvent CanStepInto_Event = new AutoResetEvent(false);
		private static DebuggerImpl? Test_DebuggerImpl;
		private static DbgCodeBreakpointsService? dbgCodeBreakpointsService;

		private static int Log_Num = 1;

		private static ESpecial_Check_Step Special_Check_Step;
		private static String? LastModuleName;
		public static void Set_StepInto() {
			//自动化有bug 先注释函数
			CanStepInto_Event.Set();
		}

		public static void Wait_Execute() {
			//自动化有bug 先注释函数
			CanStepInto_Event.WaitOne();
			Thread.Sleep(10);
		}

		public static void Reset_StepInto() {
			//自动化有bug 先注释函数
			CanStepInto_Event.Reset();
		}

		public static void Init_DebuggerImpl(DebuggerImpl _debuggerImpl, DbgCodeBreakpointsService _dbgCodeBreakpointsService) {
			//自动化有bug 先注释函数
			Test_DebuggerImpl = _debuggerImpl;
			dbgCodeBreakpointsService = _dbgCodeBreakpointsService;
			if (ModuleDef_Into != null) {
				SetModuleInto(ModuleDef_Into);
			}
		}

		public static void SetModuleInto(ModuleDef moduleDef) {
			//自动化有bug 先注释函数
			foreach (TypeDef typeDef in moduleDef.Types) {
				// Skip the <Module> class
				if (typeDef.IsGlobalModuleType) continue;

				foreach (MethodDef method in typeDef.Methods) {
					if (method.Body != null && method.Body.Instructions.Count > 10) {

						SetMethodInto(moduleDef, method);
					}
				}

				foreach (TypeDef nestedType in typeDef.NestedTypes) {

					if (nestedType.IsGlobalModuleType) continue;

					foreach (MethodDef method in nestedType.Methods) {
						if (method.Body != null && method.Body.Instructions.Count > 10) {
							SetMethodInto(moduleDef, method);
						}
					}
				}
			}
		}

		private static bool SetMethodInto(ModuleDef moduleDef, MethodDef methodDef) {
			//自动化有bug 先注释函数
			if (dbgCodeBreakpointsService != null && Test_DebuggerImpl != null) {

				if (moduleDef.Characteristics.HasFlag(Characteristics.Dll)) {
					if (MemModuleId != null) {
						dbgCodeBreakpointsService.Add(new DbgCodeBreakpointInfo(new DbgDotNetCodeLocationFactoryImpl(Test_DebuggerImpl.dbgManager).Create(MemModuleId.Value, methodDef.MDToken.Raw, 0), new DbgCodeBreakpointSettings() { IsEnabled = true }));
					}
				}
				else {
					dbgCodeBreakpointsService.Add(new DbgCodeBreakpointInfo(new DbgDotNetCodeLocationFactoryImpl(Test_DebuggerImpl.dbgManager).Create(ModuleId.Create(moduleDef.Location), methodDef.MDToken.Raw, 0), new DbgCodeBreakpointSettings() { IsEnabled = true }));
				}
				return true;
			}
			return false;


			return true;
		}

		public static void StepInto() {
			//自动化有bug 先注释函数
			switch (Special_Check_Step) {
			case ESpecial_Check_Step.IsSpecialMethod:
				//Test_DebuggerImpl?.StepOver();
				break;
			case ESpecial_Check_Step.IsSampleModule:
				Test_DebuggerImpl?.StepIntoCurrentProcess();
				break;
			case ESpecial_Check_Step.IsMemoryModule:
				Test_DebuggerImpl?.StepIntoCurrentProcess();
				break;
			case ESpecial_Check_Step.NothingSpecial:
				Test_DebuggerImpl?.StepOverCurrentProcess();
				break;
			default:
				break;
			}
		}

		public static void Special_Check(String new_module_name, MethodDef methodDef) {
			//自动化有bug 先注释函数
			if (LastModuleName == null) {
				LastModuleName = new_module_name;
			}
			Special_Check_Step = String.Equals(LastModuleName, new_module_name) ? ESpecial_Check_Step.IsSampleModule : ESpecial_Check_Step.NothingSpecial;

			if (isMemoryModule && MemModuleId != null) {
				Special_Check_Step = String.Equals(MemModuleId.Value.ModuleName, new_module_name) ? ESpecial_Check_Step.IsMemoryModule : ESpecial_Check_Step.NothingSpecial;
			}

			String[] specialMethods = { ".Form" , ".SnakeBox" };
			foreach (var specialMethod in specialMethods) {
				if (methodDef.ToString().Contains(specialMethod) == true) {
					Special_Check_Step = ESpecial_Check_Step.IsSpecialMethod;
				}
			}
		}

		public static void Log_Something(String method_str, String parameter) {
			try {
				String path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); //+ ("\\dnspu_log_{0}.txt", i);
				using (StreamWriter outputFile = new StreamWriter(Path.Combine(path, "dnsp_log.txt"), true)) {
					outputFile.Write("num({0}) ", Log_Num);
					outputFile.Write("method_str:");
					outputFile.Write(method_str);

					outputFile.Write("  ");

					outputFile.Write("parameter:");
					outputFile.Write(parameter);

					outputFile.WriteLine("\r\n");
					outputFile.Close();
					Log_Num++;
				}
			}
			catch (Exception) {

				throw;
			}

		}


		public class DbgDotNetCodeLocationImpl : DbgDotNetCodeLocation {
			public override string Type => PredefinedDbgCodeLocationTypes.DotNet;
			public override ModuleId Module { get; }
			public override uint Token { get; }
			public override uint Offset { get; }
			public override DbgILOffsetMapping ILOffsetMapping { get; }
			public override DbgModule? DbgModule => null;
			public override DbgDotNetNativeFunctionAddress NativeAddress => DbgDotNetNativeFunctionAddress.None;

			internal DbgBreakpointLocationFormatterImpl? Formatter { get; set; }
			readonly DbgDotNetCodeLocationFactoryImpl factory;

			public DbgDotNetCodeLocationImpl(DbgDotNetCodeLocationFactoryImpl factory, ModuleId module, uint token, uint offset, DbgILOffsetMapping ilOffsetMapping) {
				this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
				Module = module;
				Token = token;
				Offset = offset;
				ILOffsetMapping = ilOffsetMapping;
			}


			public override DbgCodeLocation Clone() => factory.Create(Module, Token, Offset, ILOffsetMapping);
			public override void Close() => factory.DbgManager.Value.Close(this);
			protected override void CloseCore(DbgDispatcher dispatcher) { }

			public override bool Equals(object? obj) =>
				obj is DbgDotNetCodeLocationImpl other &&
				Module == other.Module &&
				Token == other.Token &&
				Offset == other.Offset;

			public override int GetHashCode() => Module.GetHashCode() ^ (int)Token ^ (int)Offset;

			internal class DbgBreakpointLocationFormatterImpl {
			}

		}
		public class DbgDotNetCodeLocationFactoryImpl : DbgDotNetCodeLocationFactory {
			internal Lazy<DbgManager> DbgManager { get; }

			public DbgDotNetCodeLocationFactoryImpl(Lazy<DbgManager> dbgManager) => DbgManager = dbgManager;

			public override DbgDotNetCodeLocation Create(ModuleId module, uint token, uint offset, DbgILOffsetMapping ilOffsetMapping) =>
				new DbgDotNetCodeLocationImpl(this, module, token, offset, ilOffsetMapping);
		}
	}


}
