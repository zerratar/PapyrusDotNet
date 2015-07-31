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
				.function _UtilizeDelegate_b__1_0
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
						Jump _label15
					_label15:
						Return None
					.endCode
				.endFunction
				.function _UtilizeDelegate_b__1_1
					.userFlags 0
					.docString ""
					.return None
					.paramTable
					.endParamTable
					.localTable
						.local ::NoneVar None
					.endLocalTable
					.code
						CallStatic Debug Trace ::NoneVar "Second awesome was used!" 0
						Jump _label15
					_label15:
						Return None
					.endCode
				.endFunction
				.function UtilizeDelegate
					.userFlags 0
					.docString ""
					.return None
					.paramTable
					.endParamTable
					.localTable
						.local ::NoneVar None
						.local V_0 Int
						.local V_1 Int
					.endLocalTable
					.code
						Jump _label32
					_label32:
						Jump _label64
					_label64:
						CallMethod _UtilizeDelegate_b__1_0 self ::NoneVar 
						CallMethod _UtilizeDelegate_b__1_1 self ::NoneVar 
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