.info
	.source "PapyrusDotNet-Generated.psc"
	.modifyTime 1438600387
	.compileTime 1438600387
	.user "Karlj"
	.computer "CD197"
.endInfo
.userFlagsRef
	.flag conditional 1
	.flag hidden 0
.endUserFlagsRef
.objectTable
.object Example1_DelegateTesting
		.userFlags 0
		.docString ""
		.autoState
		.variableTable
			.variable ::magic String
				.userFlags 0
				.initialValue None
			.endVariable
		.endVariableTable
		.propertyTable
		.endPropertyTable
		.stateTable
			.state
				.function _UtilizeDelegate4_b__0
					.userFlags 0
					.docString ""
					.return None
					.paramTable
					.endParamTable
					.localTable
						.local ::NoneVar None
						.local V_0 Int
					.endLocalTable
					.code
						Jump _label27
					_label27:
						CallMethod _UtilizeDelegate4_b__1 self ::NoneVar ::magic
						Return None
					.endCode
				.endFunction
				.function _UtilizeDelegate4_b__1 static
					.userFlags 0
					.docString ""
					.return None
					.paramTable
						.param s String
					.endParamTable
					.localTable
						.local ::NoneVar None
						.local ::temp0 String
					.endLocalTable
					.code
						StrCat ::temp0 ::temp0 "UtilizeDelegate4 was used!"
						StrCat ::temp0 ::temp0 s
						CallStatic Debug Trace ::NoneVar 0
						Return None
					.endCode
				.endFunction
				.function UtilizeDelegate4
					.userFlags 0
					.docString ""
					.return None
					.paramTable
					.endParamTable
					.localTable
						.local ::NoneVar None
						.local V_0 Int
					.endLocalTable
					.code
						Assign ::magic "helloo"
						CallMethod _UtilizeDelegate4_b__0 self ::NoneVar 
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
						Return None
					.endCode
				.endFunction
			.endState
		.endStateTable
	.endObject
.endObjectTable