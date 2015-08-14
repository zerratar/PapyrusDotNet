.info
	.source "PapyrusDotNet-Generated.psc"
	.modifyTime 1439547956
	.compileTime 1439547956
	.user "Karlj"
	.computer "CD197"
.endInfo
.userFlagsRef
	.flag conditional 1
	.flag hidden 0
.endUserFlagsRef
.objectTable
.object Example1_TestingLinqStuff ObjectReference
		.userFlags 0
		.docString ""
		.autoState
		.variableTable
		.endVariableTable
		.propertyTable
		.endPropertyTable
		.stateTable
			.state
				.function FirstOrDefault
					.userFlags 0
					.docString ""
					.return String
					.paramTable
						.param source String[]
					.endParamTable
					.localTable
						.local V_0 String
						.local V_1 String
						.local V_2 String[]
						.local V_3 Int
						.local V_4 Bool
						.local V_5 String
						.local ::temp1 Int
						.local ::temp0 Bool
					.endLocalTable
					.code
						Assign V_2 source
						Assign V_3 0
						Jump _label43
					_label8:
						ArrayGetElement V_0 V_2 V_3
						CallMethod _FirstTest_b__0 self ::temp0 V_0
						Cast V_4 ::temp0
						CompareEQ V_4 ::temp0 0
						JumpF V_4 _label38
						Assign V_1 V_0
					_label38:
						IAdd V_3 V_3 1
					_label43:
						ArrayLength ::temp1 V_2
						CompareLT V_4 V_3 ::temp1
						JumpF V_4 _label8
						Assign V_1 V_5
						Jump _label68
					_label68:
						Return V_1
					.endCode
				.endFunction
				.function _FirstTest_b__0
					.userFlags 0
					.docString ""
					.return Bool
					.paramTable
						.param j String
					.endParamTable
					.localTable
						.local V_0 Bool
						.local ::temp0 Bool
					.endLocalTable
					.code
						CompareEQ ::temp0 j "5"
						Assign V_0 ::temp0
						Jump _label14
					_label14:
						Return V_0
					.endCode
				.endFunction
				.function _FirstTest_b__1
					.userFlags 0
					.docString ""
					.return Bool
					.paramTable
						.param l String
					.endParamTable
					.localTable
						.local V_0 Bool
						.local ::temp0 Bool
					.endLocalTable
					.code
						CompareEQ ::temp0 l ""
						Assign V_0 ::temp0
						Jump _label14
					_label14:
						Return V_0
					.endCode
				.endFunction
				.function FirstTest
					.userFlags 0
					.docString ""
					.return None
					.paramTable
					.endParamTable
					.localTable
						.local ::NoneVar None
						.local V_0 String[]
						.local V_1 String
						.local V_2 String
						.local V_3 String[]
						.local ::temp1 String
						.local ::temp0 String
					.endLocalTable
					.code
						ArrayCreate V_3 10
						ArraySetElement V_3 0 "0"
						ArraySetElement V_3 1 "1"
						ArraySetElement V_3 2 "2"
						ArraySetElement V_3 3 "3"
						ArraySetElement V_3 4 "4"
						ArraySetElement V_3 5 "5"
						ArraySetElement V_3 6 "6"
						ArraySetElement V_3 7 "7"
						ArraySetElement V_3 8 "8"
						ArraySetElement V_3 9 "9"
						Assign V_0 V_3
						JumpF V_0 _label119
						Jump _label119
					_label119:
						CallMethod FirstOrDefault self V_1 
						StrCat ::temp0 ::temp0 "We selected number "
						StrCat ::temp0 ::temp0 V_1
						CallStatic Debug Trace ::NoneVar 0
						JumpF V_0 _label175
						Jump _label175
					_label175:
						CallMethod LastOrDefault self V_2 
						StrCat ::temp1 ::temp1 "Our last number is "
						StrCat ::temp1 ::temp1 V_2
						CallStatic Debug Trace ::NoneVar 0
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