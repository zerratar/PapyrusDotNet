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
	.object Example1_GodModeExtended Example1_GodMode
		.userFlags 0
		.docString ""
		.autoState
		.variableTable
			.variable ::MyActors Actor[]
				.userFlags 0
				.initialValue None
			.endVariable
		.endVariableTable
		.propertyTable
			.property pMyActors Actor[] auto
				.userFlags 0
				.docString ""
				.autoVar ::MyActors
			.endProperty
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
					.endLocalTable
					.code
						Return None
					.endCode
				.endFunction
				.function get_Item
					.userFlags 0
					.docString ""
					.return Actor
					.paramTable
						.param idx Int
					.endParamTable
					.localTable
						.local V_0 Actor
					.endLocalTable
					.code
						ArrayGetElement V_0 ::MyActors idx
						Jump _label12
					_label12:
						Return V_0
					.endCode
				.endFunction
			.endState
		.endStateTable
	.endObject
.endObjectTable