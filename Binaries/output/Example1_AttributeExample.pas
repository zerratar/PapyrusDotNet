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
.object Example1_AttributeExample Actor
		.userFlags 3
		.docString ""
		.autoState
		.variableTable
			.variable ::MyPropertyString String
				.userFlags 0
				.initialValue None
			.endVariable
			.variable ::WeaponRef Weapon
				.userFlags 0
				.initialValue None
			.endVariable
			.variable ::PlayerRef Actor
				.userFlags 0
				.initialValue None
			.endVariable
			.variable ::totalHoursElapsed Int
				.userFlags 0
				.initialValue 0
			.endVariable
			.variable ::dummy String
				.userFlags 0
				.initialValue "Hello world!"
			.endVariable
		.endVariableTable
		.propertyTable
			.property pMyPropertyString String auto
				.userFlags 0
				.docString ""
				.autoVar ::MyPropertyString
			.endProperty
			.property pWeaponRef Weapon auto
				.userFlags 0
				.docString ""
				.autoVar ::WeaponRef
			.endProperty
			.property pPlayerRef Actor auto
				.userFlags 0
				.docString ""
				.autoVar ::PlayerRef
			.endProperty
		.endPropertyTable
		.stateTable
			.state
				.function OnUpdateGameTime
					.userFlags 0
					.docString ""
					.return None
					.paramTable
					.endParamTable
					.localTable
						.local ::NoneVar None
						.local V_0 Int
						.local ::temp1 String
						.local ::temp0 String
					.endLocalTable
					.code
						Assign V_0 ::totalHoursElapsed
						IAdd ::totalHoursElapsed V_0 1
						CallMethod GetName ::PlayerRef ::temp0 
						Cast ::temp1 ::totalHoursElapsed
						StrCat ::temp1 ::temp1 ::totalHoursElapsed
						StrCat ::temp1 ::temp1 " hours spent ingame! And my name is "
						StrCat ::temp1 ::temp1 ::temp0
						CallStatic Debug MessageBox ::NoneVar ::temp1
						CallMethod RegisterForSingleUpdateGameTime self ::NoneVar 1
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
					.endLocalTable
					.code
						CallMethod __ctor self ::NoneVar
						CallMethod RegisterForSingleUpdateGameTime self ::NoneVar 1
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
						Assign ::totalHoursElapsed 0
						Return None
					.endCode
				.endFunction
			.endState
		.endStateTable
	.endObject
.endObjectTable