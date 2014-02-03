.info
	.source "PapyrusDotNet-Generated.psc"
	.modifyTime 1391440704
	.compileTime 1391440704
	.user "Karlj"
	.computer "CD197"
.endInfo
.userFlagsRef
	.flag conditional 1
	.flag hidden 0
.endUserFlagsRef
.objectTable
	.object Example1_GodMode Actor
		.userFlags 0
		.docString ""
		.autoState
		.variableTable
			.variable ::myGodMode Example1_GodMode
				.userFlags 0
				.initialValue None
			.endVariable
		.endVariableTable
		.propertyTable
		.endPropertyTable
		.stateTable
			.state
				.function test
					.userFlags 0
					.docString ""
					.return None
					.paramTable
					.endParamTable
					.localTable
						.local ::NoneVar None
						.local V_0 Example1_GodMode[]
						.local V_1 Example1_GodMode
						.local V_2 Example1_GodMode[]
						.local V_3 Int
						.local V_4 Bool
						.local ::temp0 Int
					.endLocalTable
					.code
						ArrayCreate V_0 120
						Assign V_2 V_0
						Assign V_3 0
						Jump _label38
					_label16:
						ArrayGetElement V_1 V_2 V_3
						CallMethod SetName V_1 ::NoneVar "hello!"
						IAdd V_3 V_3 1
					_label38:
						ArrayLength ::temp0 V_2
						CompareLT V_4 V_3 ::temp0
						JumpF V_4 _label16
						Return None
					.endCode
				.endFunction
				.function OnInit
					.userFlags 0
					.docString ""
					.return None
					.paramTable
					.endParamTable
					.localTable
						.local ::NoneVar None
						.local V_0 Float
						.local V_1 Int
						.local V_2 Example1_GodMode[]
						.local V_3 Int
						.local V_4 Example1_GodMode
						.local V_5 Float[]
						.local V_6 Float
						.local V_7 Bool
						.local V_8 Int
						.local ::temp2 Int
						.local ::temp1 Bool
						.local ::temp0 Bool
					.endLocalTable
					.code
						Assign V_0 123
						Assign V_1 V_0
						CompareEQ V_7 V_1 0
						JumpF V_7 _label36
						CallStatic Debug MessageBox ::NoneVar "HELLOOO 2"
					_label36:
						Assign V_8 V_1
						CompareEQ ::temp0 77 V_8
						JumpT ::temp0 _label66
						CompareEQ ::temp1 123 V_8
						JumpT ::temp1 _label53
						Jump _label79
					_label53:
						CallStatic Bool MessageBox ::NoneVar "123"
						Jump _label121
					_label66:
						CallStatic Debug MessageBox ::NoneVar "HELLOOO"
						Jump _label79
					_label79:
						ArrayCreate V_2 120
						ArrayLength ::temp2 V_2
						Assign V_3 ::temp2
						ArrayGetElement V_4 V_2 28
						ArraySetElement V_2 28 None
						ArrayCreate V_5 128
						ArrayGetElement V_6 V_5 28
					_label121:
						Return None
					.endCode
				.endFunction
				.function ActivateGodMode
					.userFlags 0
					.docString ""
					.return None
					.paramTable
						.param player Actor
					.endParamTable
					.localTable
						.local ::NoneVar None
						.local V_0 Weapon
					.endLocalTable
					.code
						CallMethod GetEquippedWeapon player V_0 False
						CallMethod SetBaseDamage V_0 ::NoneVar 9999
						CallMethod SetActorValue player ::NoneVar "Health" 999999
						CallMethod SetActorValue player ::NoneVar "Magicka" 999999
						CallMethod SetActorValue player ::NoneVar "Stamina" 999999
						CallStatic Debug MessageBox ::NoneVar "God Mode activated!"
						Return None
					.endCode
				.endFunction
			.endState
		.endStateTable
	.endObject
.endObjectTable