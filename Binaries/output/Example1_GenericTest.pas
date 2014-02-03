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
	.object Example1_GenericTest 
		.userFlags 0
		.docString ""
		.autoState
		.variableTable
			.variable ::boolgen Example1_GenericClass_Bool
				.userFlags 0
				.initialValue None
			.endVariable
			.variable ::boolgenProp Example1_GenericClass_Bool
				.userFlags 0
				.initialValue None
			.endVariable
		.endVariableTable
		.propertyTable
			.property pboolgenProp Example1_GenericClass_Bool auto
				.userFlags 0
				.docString ""
				.autoVar ::boolgenProp
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
						.local ::NoneVar None
						.local V_0 Example1_GenericClass_Form
						.local V_1 Example1_GenericClass_Int
					.endLocalTable
					.code
						CallMethod Set V_0 ::NoneVar 
						CallMethod Set V_1 ::NoneVar 9999
						Return None
					.endCode
				.endFunction
			.endState
		.endStateTable
	.endObject
.endObjectTable