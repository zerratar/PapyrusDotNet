.info
	.source "PapyrusDotNet-Generated.psc"
	.modifyTime 1438353026
	.compileTime 1438353026
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
		.endVariableTable
		.propertyTable
		.endPropertyTable
		.stateTable
			.state
				.function UtilizeDelegate
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
						CallMethod Invoke V_0 ::NoneVar 
						Return None
					.endCode
				.endFunction
				.function <UtilizeDelegate>b__0 static
					.userFlags 0
					.docString ""
					.return None
					.paramTable
					.endParamTable
					.localTable
						.local ::NoneVar None
					.endLocalTable
					.code
						CallStatic Debug Trace ::NoneVar "Awesome was used!" 0
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