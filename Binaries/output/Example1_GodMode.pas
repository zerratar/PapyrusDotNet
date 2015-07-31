.info
	.source "PapyrusDotNet-Generated.psc"
	.modifyTime 1438366422
	.compileTime 1438366422
	.user "Karl"
	.computer "Z-PC"
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
						.local ::NoneVar None
					.endLocalTable
					.code
						CallMethod __ctor self ::NoneVar
						CallMethod ActivateGodMode self ::NoneVar 
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
				.function __ctor
					.userFlags 0
					.docString ""
					.return None
					.paramTable
					.endParamTable
					.localTable
						.local ::NoneVar None
					.endLocalTable
					.code
						Return None
					.endCode
				.endFunction
			.endState
		.endStateTable
	.endObject
.endObjectTable