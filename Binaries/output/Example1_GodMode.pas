.info
	.source "PapyrusDotNet-Generated.psc"
	.modifyTime 1391091084
	.compileTime 1391091084
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
				.function OnInit
					.userFlags 0
					.docString ""
					.return None
					.paramTable
					.endParamTable
					.localTable
						.local V_0 Object
						.local V_1 Int
						.local V_2 Example1_GodMode[]
						.local V_3 Int
						.local V_4 Example1_GodMode
						.local V_5 Float[]
						.local V_6 Float
					.endLocalTable
					.code
						Assign V_0 123
						Assign V_1 V_0
						ArrayCreate V_2 120
						ArrayLength V_3 V_2
						ArrayGetElement V_4 V_2 28
						ArraySetElement V_2 28 None
						ArrayCreate V_5 128
						ArrayGetElement V_6 V_5 28
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