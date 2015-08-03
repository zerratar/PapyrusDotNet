.info
	.source "PapyrusDotNet-Generated.psc"
	.modifyTime 1438617588
	.compileTime 1438617588
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
			.variable ::whatHorrorLiesHere String
				.userFlags 0
				.initialValue None
			.endVariable
			.variable ::magic String
				.userFlags 0
				.initialValue None
			.endVariable
		.endVariableTable
		.propertyTable
		.endPropertyTable
		.stateTable
			.state
				.function _UtilizeDelegate2_b__4
					.userFlags 0
					.docString ""
					.return None
					.paramTable
					.endParamTable
					.localTable
						.local ::NoneVar None
						.local ::temp0 String
					.endLocalTable
					.code
						StrCat ::temp0 ::temp0 "UtilizeDelegate2 was used!"
						StrCat ::temp0 ::temp0 ::whatHorrorLiesHere
						CallStatic Debug Trace ::NoneVar 0
						Return None
					.endCode
				.endFunction
				.function _UtilizeDelegate4_b__9
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
						Jump _label27
					_label27:
						Assign V_0 None
						CallMethod _UtilizeDelegate4_b__a self ::NoneVar ::magic
						Return None
					.endCode
				.endFunction
				.function _UtilizeDelegate4_b__a
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
				.function _UtilizeDelegate_b__0
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
				.function _UtilizeDelegate_b__1
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
						Return None
					.endCode
				.endFunction
				.function _UtilizeDelegate3_b__7
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
						StrCat ::temp0 ::temp0 "UtilizeDelegate3 was used!"
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
						CallMethod _UtilizeDelegate4_b__9 self ::NoneVar 
						Return None
					.endCode
				.endFunction
				.function UtilizeDelegate3
					.userFlags 0
					.docString ""
					.return None
					.paramTable
					.endParamTable
					.localTable
						.local ::NoneVar None
						.local V_0 String
						.local V_1 Int
					.endLocalTable
					.code
						Assign V_0 "test"
						Jump _label33
						Jump _label33
					_label33:
						Assign V_1 None
						CallMethod _UtilizeDelegate3_b__7 self ::NoneVar V_0
						Return None
					.endCode
				.endFunction
				.function UtilizeDelegate2
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
						Assign ::whatHorrorLiesHere "test123"
						CallMethod _UtilizeDelegate2_b__4 self ::NoneVar 
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
						Jump _label27
						Jump _label27
					_label27:
						Assign V_0 None
						Jump _label59
						Jump _label59
					_label59:
						Assign V_1 None
						CallMethod _UtilizeDelegate_b__0 self ::NoneVar 
						CallMethod _UtilizeDelegate_b__1 self ::NoneVar 
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