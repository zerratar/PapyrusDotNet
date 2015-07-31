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
.object Example1_GenericTest
		.userFlags 0
		.docString ""
		.autoState
		.variableTable
			.variable ::genericInteger Example1_GenericClass_Int
				.userFlags 0
				.initialValue None
			.endVariable
		.endVariableTable
		.propertyTable
			.property pgenericInteger Example1_GenericClass_Int auto
				.userFlags 0
				.docString ""
				.autoVar ::genericInteger
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
						.local V_0 Int
						.local ::temp0 String
					.endLocalTable
					.code
						CallMethod __ctor self ::NoneVar
						CallMethod Set ::genericInteger ::NoneVar 9999
						CallMethod Get ::genericInteger V_0 
						StrCat ::temp0 ::temp0 "The value is: "
						Cast ::temp0 V_0
						StrCat ::temp0 ::temp0 V_0
						CallStatic Debug Trace ::NoneVar 0
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