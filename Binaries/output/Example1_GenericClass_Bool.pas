.info
	.source "PapyrusDotNet-Generated.psc"
	.modifyTime 1391285749
	.compileTime 1391285749
	.user "Karlj"
	.computer "CD197"
.endInfo
.userFlagsRef
	.flag conditional 1
	.flag hidden 0
.endUserFlagsRef
.objectTable
	.object Example1_GenericClass_Bool 
		.userFlags 0
		.docString ""
		.autoState
		.variableTable
			.variable ::GenericVariable Bool
				.userFlags 0
				.initialValue None
			.endVariable
		.endVariableTable
		.propertyTable
		.endPropertyTable
		.stateTable
			.state
				.function Set
					.userFlags 0
					.docString ""
					.return None
					.paramTable
						.param value Bool
					.endParamTable
					.localTable
					.endLocalTable
					.code
						Assign ::GenericVariable value
						Return None
					.endCode
				.endFunction
				.function Get
					.userFlags 0
					.docString ""
					.return Bool
					.paramTable
					.endParamTable
					.localTable
						.local V_0 Bool
					.endLocalTable
					.code
						Assign V_0 ::GenericVariable
						Jump _label10
					_label10:
						Return V_0
					.endCode
				.endFunction
			.endState
		.endStateTable
	.endObject
.endObjectTable