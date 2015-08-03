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
.object Example1_TestingLinqStuff
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
						.param predicate Func`2
					.endParamTable
					.localTable
						.local V_0 String
						.local V_1 String
						.local V_2 String[]
						.local V_3 Int
						.local V_4 Bool
						.local V_5 String
						.local ::temp1 Int
						.local ::temp0 !1
					.endLocalTable
					.code
						Assign V_2 source
						Assign V_3 0
						Jump _label43
					_label8:
						ArrayGetElement V_0 V_2 V_3
						CallMethod Invoke predicate ::temp0 V_0
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
				.function FirstTest
					.userFlags 0
					.docString ""
					.return None
					.paramTable
					.endParamTable
					.localTable
						.local V_0 String[]
						.local V_1 String
						.local V_2 String[]
					.endLocalTable
					.code
						ArrayCreate V_2 10
						ArraySetElement V_2 0 "0"
						ArraySetElement V_2 1 "1"
						ArraySetElement V_2 2 "2"
						ArraySetElement V_2 3 "3"
						ArraySetElement V_2 4 "4"
						ArraySetElement V_2 5 "5"
						ArraySetElement V_2 6 "6"
						ArraySetElement V_2 7 "7"
						ArraySetElement V_2 8 "8"
						ArraySetElement V_2 9 "9"
						Assign V_0 V_2
						JumpF V_0 _label119
						Jump _label119
					_label119:
						CallMethod FirstOrDefault self V_1 
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