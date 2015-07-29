.info
	.source "PapyrusDotNet-Generated.psc"
	.modifyTime 1438182744
	.compileTime 1438182744
	.user "Karlj"
	.computer "CD197"
.endInfo
.userFlagsRef
	.flag conditional 1
	.flag hidden 0
.endUserFlagsRef
.objectTable
	.object Example1_FacialExpressions Actor
		.userFlags 0
		.docString ""
		.autoState
		.variableTable
			.variable ::playerExpression Example1_Face
				.userFlags 0
				.initialValue None
			.endVariable
			.variable ::activeExpression Int
				.userFlags 0
				.initialValue None
			.endVariable
			.variable ::expressionCount Int
				.userFlags 0
				.initialValue None
			.endVariable
			.variable ::lastUpdate Float
				.userFlags 0
				.initialValue None
			.endVariable
		.endVariableTable
		.propertyTable
			.property pplayerExpression Example1_Face auto
				.userFlags 0
				.docString ""
				.autoVar ::playerExpression
			.endProperty
		.endPropertyTable
		.stateTable
			.state
				.function OnUpdateGameTime
					.userFlags 0
					.docString ""
					.return None
					.paramTable
					.endParamTable
					.localTable
						.local ::NoneVar None
						.local V_0 Bool
						.local ::temp3 String
						.local ::temp2 Int
						.local ::temp1 Int
						.local ::temp0 Float
					.endLocalTable
					.code
						CallStatic DateTime get_Now ::temp0 
						Cast ::temp2 ::lastUpdate
						ISubtract V_0 ::lastUpdate ::temp1
						JumpF V_0 _label100
						CompareLT V_0 ::activeExpression ::expressionCount
						JumpF V_0 _label69
						IAdd ::activeExpression ::activeExpression 1
						Jump _label76
					_label69:
						Assign ::activeExpression 0
					_label76:
						Assign ::playerExpression ::activeExpression
						CallStatic DateTime get_Now ::lastUpdate ::temp0
					_label100:
						CallMethod RegisterForSingleUpdateGameTime self ::NoneVar 1
						StrCat ::temp3 ::temp3 "New expression: "
						Cast ::temp3 ::activeExpression
						StrCat ::temp3 ::temp3 ::activeExpression
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
						CallMethod __ctor self ::NoneVar
						CallMethod RegisterForSingleUpdateGameTime self ::NoneVar 1
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
						Assign ::expressionCount 8
						CallStatic DateTime get_Now ::lastUpdate 
						Return None
					.endCode
				.endFunction
			.endState
		.endStateTable
	.endObject
.endObjectTable