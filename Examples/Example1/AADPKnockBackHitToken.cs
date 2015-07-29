
using PapyrusDotNet.Core;

public class AADPKnockBackHitToken : ActiveMagicEffect
{
	private int result, HurtMe, HasMPerk, hitnow, random, WeaponAttackType, LogicalWards, wardblockswitch;

	private double WardBlock, Astrength, Distance, Momentum,
				timer, Impulse,
				AngleZ, deadlybashperk,
				RearBashBonus, AttackerWeaponMass,
				TopHeavy, Force,
				Mass, MassMod,
				BaseStamina, BowDrawing,
				Wforce, FemaleMult,
				MeStrength, MessagePercent,
				SecondsPassed, SmashGlobal,
				SteelAxeW, SteelBattleaxeW,
				SteelDaggerW, SteelGreatswordW,
				SteelSwordW, Delay,
				PreStamina;


	[Property, Auto]
	public MiscObject aadpKnockBackHitToken;

	[Property, Auto]
	public Faction aadpLocationHit, aadpGhostFaction, aadpHadJustFallen, aadpBleedOut, aadpICanBashGhost;

	[Property, Auto]
	public Perk ReflectBlows, ShieldCharge, deadlybash, BoneBreaker30, DeepWounds60, BoneBreaker90, Bladesman90;

	[Property, Auto]
	public Keyword ActorTypeNPC, ActorTypeUndead, Vampire, weapMaterialSilver;

	[Property, Auto]
	public MagicEffect aadpFortifyCarryWeight;

	[Property, Auto]
	public Race WerewolfBeastRace;

	[Property, Auto]
	private Actor PlayerREF;

	[Property, Auto]
	public GlobalVariable aadpBlockStaminaMul, aadpClassicVampires, aadpMoMessageMin, aadpTitanSmash, aadpLogicalWards;

	[Property, Auto]
	public Sound WackSound;

	[Property, Auto]
	public Spell aadpSlightFakeWeaponHit, aadpFakeWeaponHit, aadpMedFakeWeaponHit, aadpBleedOutSpl, aadpConfused;

	[Property, Auto]
	public Weapon SteelAxe, SteelBattleaxe, SteelDagger, SteelGreatsword, SteelSword;

	private Weapon AttackerWeapon, LastAttackWeapon;

	private ActorBase ActorRefBase;

	private Actor LastAttacker, Me, Attacker;

	private bool IsStaggered, MeHasBow, GhostFaction, HitBlocked, PowerAttack, BashAttack;

	private Form Source;

	private string PlayerWrap;


	public override void OnEffectStart(Actor akTarget, Actor akCaster)
	{
		Me = akTarget;
		RegisterForActorAction(5);
		RegisterForActorAction(6);


		BaseStamina = Me.GetBaseActorValue("stamina");
		MeStrength = Me.GetActorValue("RightMobilityCondition");
		GhostFaction = Me.IsInFaction(aadpGhostFaction);
		SteelSwordW = SteelSword.GetWeight();
		SteelDaggerW = SteelDagger.GetWeight();
		SteelAxeW = SteelAxe.GetWeight();
		SteelGreatswordW = SteelGreatsword.GetWeight();
		SteelBattleaxeW = SteelBattleaxe.GetWeight();

		MessagePercent = aadpMoMessageMin.GetValue();

		PlayerWrap = PlayerREF.GetLeveledActorBase().GetName() + " threw a Wrap Shot.";

		SmashGlobal = aadpTitanSmash.GetValue();

		if (Me.HasPerk(deadlybash))
			deadlybashperk = 0.5;
		else
			deadlybashperk = 1;


		MeHasBow = Me.GetEquippedWeapon(false).IsBow();

		LogicalWards = (int)aadpLogicalWards.GetValue();
		GotoState("NormalState");
	}



	/*



State NormalState

Event OnObjectequipped(Form akBaseObject, ObjectReference akReference)

        basestamina = me.getbaseactorvalue("stamina")
        MessagePercent = aadpMoMessageMin.GetValue()
        SteelSwordW = SteelSword.GetWeight()
        SteelDaggerW = SteelDagger.GetWeight()
        SteelAxeW = SteelAxe.GetWeight()
        SteelGreatSwordW = SteelGreatSword.GetWeight()
        SteelBattleaxeW = SteelBattleaxe.GetWeight()
        GhostFaction = me.isinfaction(aadpGhostFaction)
        SmashGlobal = aadpTitanSmash.GetValue()

if (me.hasperk(DeadlyBash))
deadlybashperk = 0.5
else
deadlybashperk = 1
endif

LogicalWards = aadpLogicalWards.getvalue() as int

MeStrength = Me.GetActorValue("RightMobilityCondition")

MeHasBow = (me.GetEquippedWeapon().IsBow())

EndEvent




Event OnObjectunequipped(Form akBaseObject, ObjectReference akReference)
MeHasBow = (me.GetEquippedWeapon().IsBow())
MeStrength = Me.GetActorValue("RightMobilityCondition")
EndEvent



Event OnActorAction(int actionType, Actor akActor, Form TheSource, int slot)

if akActor == me
if actiontype == 5 ;--Draw Begin
BowDrawing = 0.2
elseif actiontype == 6 ;-- Bow Release
BowDrawing = 1.0
endif
endif

EndEvent








Event OnHit(ObjectReference akAggressor, Form akSource, Projectile akProjectile, Bool abPowerAttack, Bool abSneakAttack, Bool abBashAttack, Bool abHitBlocked)

if (source as weapon).IsBow() && !abBashAttack ;-- HitBlocked check done below with ward check
return
endif



GoToState("BusyState")

;timemark = Utility.GetCurrentRealTime()

Source = akSource
HitBlocked = abHitBlocked
BashAttack = abBashAttack
PowerAttack = abPowerAttack
Attacker = akAggressor As Actor
HitNow = 0

RunTest()

GoToState("NormalState")

endevent


endstate




Function RunTest()

WardBlock = 0
float FlankAngle = math.abs(me.GetHeadingAngle(attacker))

if HitBlocked == true
return
elseif LogicalWards == 1
WardBlock = me.getactorvalue("WardPower")
If WardBlock > 0
If FlankAngle <= 40.0
return
endif
endif

endif



AttackerWeapon = (source As Weapon)

if (AttackerWeapon As Weapon) == false
if Attacker.getAnimationVariableBool("bLeftHandAttack")
AttackerWeapon = attacker.GetEquippedWeapon(true)
else
AttackerWeapon = attacker.GetEquippedWeapon()
endif
endif



 WeaponAttackType = AttackerWeapon.GetWeaponType() ;-- also used in the script below.  This SKSE function for crossbow == 9 not 12

  int HitType = Source.GetType()

;debug.messagebox(source.getname() + "   " + Hittype)

if !BashAttack
      if HitType != 41;-- even fist and bite is type 41
              Return
       endif
endif


Distance = attacker.getdistance(me)

CalcTheHit()


endfunction










Function CalcTheHit()

RearBashBonus = 1
 Momentum = 0

if me.getAnimationVariableFLOAT("TurnDelta") || me.getAnimationVariableFLOAT("SpeedSampled")
Momentum = Attacker.GetActorValue("RightAttackCondition") ; Must do this befor bash check below
endif

float AttackerCurStamP = Attacker.GetActorValuepercentage("Stamina")
IsStaggered = Me.getAnimationVariableBool("IsStaggering")


                If Attacker.GetActorValue("EnduranceCondition") > AttackerCurStamP
                PreStamina = Attacker.GetActorValue("EnduranceCondition")
        Else
                PreStamina = AttackerCurStamP
        EndIf


Astrength = Attacker.GetActorValue("RightMobilityCondition")

if MeHasBow
if me.getAnimationVariableBool("Isattacking")
BowDrawing = 0.2
endif
endif


if WeaponAttackType != 0 && WeaponAttackType != 2
if distance < 95 * Attacker.GetScale() ;-- wrap shot
if ME.Hasspell(aadpConfused)

if attacker == playerref
debug.Notification(PlayerWrap)
endif

Astrength = Astrength * 0.5

         if PreStamina >= 0.75
               if (attacker.hasperk(BoneBreaker90) && WeaponAttackType > 4 ) || (attacker.hasperk(Bladesman90) && WeaponAttackType < 5)
                   Astrength = Astrength * 1.5
              endif
         endif

endif
endif
endif


HurtMe = 0
if !Me.isdead()
if !me.isinfaction(aadpBleedOut)
if !me.IsBleedingOut()
if Astrength * (2 + (AttackerWeapon.GetWeight()/SteelSwordW)) * (1 + (PowerAttack as int)) * (1 + (BashAttack as int)) * PreStamina > (MeStrength * 2) * BowDrawing
if BowDrawing == 1.0
Debug.SendAnimationEvent(Me, "staggerstart")
else
Debug.SendAnimationEvent(Me, "RecoilLargestart")
BowDrawing = 1.0
endif
Hurtme = 1
endif
endif
endif
endif


AngleZ = Attacker.GetAngleZ() + Attacker.GetHeadingAngle(Me)



if !BashAttack
if AttackerWeapon != LastAttackWeapon
GetWepStats()
LastAttackWeapon = AttackerWeapon
endif

else

                Force = 0.2
                Mass = 0.01
                TopHeavy =  1.1
 Endif


                 WForce = Force ;-- remember to never change "Force" so it is good for the next shot wiout a weapon recheck. WForce can be changed as seen below.





;-- 0.3 to 0.5 Seconds Script lag up to here






          HasMPerk = 0
        if BashAttack
            if attacker.hasperk(ShieldCharge)
            hasMperk = 1
            Momentum = (Momentum * 2)
            endif
        endif

              Momentum = Momentum + 1


        Astrength =  Momentum * (AStrength * (0.25 + (0.75 * PreStamina))) * (1 + (PowerAttack As Int))

        if attacker.hasperk(ReflectBlows)
        if ((me.GetWornForm(Armor.getMaskForSlot(32))as armor).islightarmor())
                AStrength = Astrength * 1.1
                elseif ((me.GetWornForm(Armor.getMaskForSlot(47))as armor).islightarmor())
                AStrength = Astrength * 1.1
                endif
                endif


       wForce = (AStrength / MeStrength) * TopHeavy * 50 * RearBashBonus

;if me == playerref || Attacker == playerref
; debug.messagebox("Wforce = " + wforce + " Astrength " + AStrength + " ME strength: " + MEStrength + " Attacker Stamina : " + PreStamina)
;endif



if aadpClassicVampires.GetValue() > 0.0
If Attacker.HasKeyword(Vampire)
wforce = wforce * 5
If wForce * 0.25 > MeStrength
WackSound.Play(Attacker)
SmashGlobal = 1
EndIf
elseif Attacker.GetRace() == WerewolfBeastRace
wforce = wforce * 5
If wForce * 0.25 > MeStrength
SmashGlobal = 1
WackSound.Play(Attacker)
EndIf
endif
endif
if (attacker.hasmagiceffect(aadpFortifyCarryWeight))
wforce = wforce * 5
If wForce * 0.25 > MeStrength
SmashGlobal = 1
WackSound.Play(Attacker)
EndIf
EndIf






;if ME.getAnimationVariableBool("bAnimationDriven") ;-- actor is NOT in havok yet
; debug.messagebox("attacker force = " + force)
;else
; debug.messagebox("actor is in havok")
;endif
;debug.messagebox("attacker strength: " + AStrength + "attacker force = " + wforce + " prestamina = " + PreStamina + " Mestrength = " + Mestrength + " Mass = " + mass + " massmod = " + massmod)

;if attacker == playerref
;debug.messagebox("attacker wforce = " + wforce + " prestamina = " + PreStamina)
;endif

;if ME.getAnimationVariableBool("bAnimationDriven") ;-- actor is NOT in havok yet NOT WORKING ALL THE TIME
;if me == playerref
;  debug.messagebox("Wforce = " + wforce + " Astrength " + AStrength + " ME strength: " + MEStrength)
;endif

;TimeLag = Utility.GetCurrentRealTime() - Timelag
;debug.messagebox(TimeLag)



if Hurtme == 1
Debug.SendAnimationEvent(Me, "staggerstop")
Debug.SendAnimationEvent(Me, "Recoilstop")
endif



if wForce *  (1 + (IsStaggered as int)) * (1 + (BashAttack as int)) > MeStrength
StaggerResult()
HitNow = 1
elseIf Attacker == PlayerREF
MomentumMessage()
endif




endfunction






function StaggerResult()

;-- 0.6 Seconds Script lag up to here


Result = 0

If wForce * 0.1 > MeStrength && MeStrength > 1 && SmashGlobal > 0

wforce = wforce * 0.5

               Impulse = (wForce / (MeStrength * 4))

                If Impulse < 3
                        Impulse = 3
                ElseIf Impulse > 18
                        Impulse = 18
                EndIf

                Impulse = 25 * Impulse * SmashGlobal


Result = 1



elseif wForce * (1 + (BashAttack as int)) * 0.4 > MeStrength

if IsStaggered

             If TopHeavy == 1.2
                           if me.getfactionrank(aadpLocationHit) == 32
                                  if WeaponAttackType == 1 || WeaponAttackType == 3 || WeaponAttackType == 4 || BashAttack
                            If Attacker.HasPerk(BoneBreaker30)
                            result = 7
                            endif
                            elseif WeaponAttackType == 6 || WeaponAttackType == 5
                            if Attacker.HasPerk(DeepWounds60)
                            result = 7
                            endif
                           endif
                           Endif
               Endif

else
Result = 8
endif


elseif wForce * (1 + (BashAttack as int)) * 0.6 > MeStrength

if !IsStaggered
Result = 9
else
Result = 10
endif


elseif IsStaggered
Result = 8


else
random = Utility.RandomInt(1, 2)
Timer =  ((wForce * (1 + (BashAttack as int)) / MeStrength) * 0.10) + 0.15
Result = 11
endif


endfunction




function StaggerThemNow()

;if attacker == playerref
;Timemark = Utility.GetCurrentRealTime() - Timemark
;debug.messagebox(Timemark)
;Timemark = -1
;endif


;-0.7   Seconds Script lag up to here




if Result == 1

                Attacker.PushActoraway(Me, 0)
                Me.ApplyHavokImpulse(Math.Sin(AngleZ), Math.Cos(AngleZ), (Impulse * 0.005), Impulse)

elseif Result == 7
attacker.DoCombatSpellApply(aadpBleedOutSpl, me)

elseif Result == 8
KnockDown()

elseif result == 9
aadpMedFakeWeaponHit.cast(Attacker, Me)

elseif result == 10
KnockDown()

elseif Result == 11

if random == 1 && !Me.isdead()
Debug.SendAnimationEvent(Me, "staggerstart")
               Utility.Wait(Timer)
;debug.messagebox("staggar")
Debug.SendAnimationEvent(Me, "staggerstop")
elseif random == 2
Debug.SendAnimationEvent(Me, "recoilstart")
               Utility.Wait(Timer)
;debug.messagebox("recoil")
Debug.SendAnimationEvent(Me, "recoilstop")

endif

random = 0
endif

Result == 0




If Attacker == PlayerREF
MomentumMessage()
endif




endfunction





Function KnockDown()

               Attacker.PushActoraway(Me, 0)
               Me.ApplyHavokImpulse(Math.Sin(AngleZ), Math.Cos(AngleZ), 1, 150)


EndFunction








Function GetWepStats()


 LastAttackWeapon = AttackerWeapon

  ; WeaponAttackType = AttackerWeapon.GetWeaponType() <---  This is checked above in the "if then return" part of the on hit block. This SKSE function for crossbow == 9 not 12

                 ;-- defualts
                MassMod = 1
                Force = 0.09
                Mass = 0.09
                TopHeavy = 1


       if WeaponAttackType == 0 ; Hand to hand

                TopHeavy = 1.2
                Force = 0.15 ;-- more push force in H2H just like in a bash
                Mass = 0.01 ;-- but still very low mass to push weapon and sheilds
        ElseIf WeaponAttackType == 2 ; One-handed dagger

                MassMod = AttackerWeapon.GetWeight() / SteelDaggerW
                Force = 0.03
                Mass = 0.03
        ElseIf WeaponAttackType == 1 ; One-handed sword

               MassMod = AttackerWeapon.GetWeight() / SteelSwordW
               ;-- defualts above

        ElseIf WeaponAttackType == 5 ; Two-handed sword

                MassMod = AttackerWeapon.GetWeight() / SteelGreatSwordW
                Force = 0.12
                Mass = 0.12
        ElseIf WeaponAttackType == 3 ; One-handed axe

                MassMod = AttackerWeapon.GetWeight() / SteelAxeW
                Force = 0.12
                Mass = 0.24
                TopHeavy = 1.2

        ElseIf WeaponAttackType == 4 ; One-handed mace

                MassMod = AttackerWeapon.GetWeight() / SteelAxeW
                Force = 0.12
                Mass = 0.24
                TopHeavy = 1.2

        ElseIf WeaponAttackType == 6 ; Two-handed axe/mace

                MassMod = AttackerWeapon.GetWeight() / SteelBattleAxeW
                Force = 0.20
                Mass = 0.36
                TopHeavy = 1.2

        EndIf

        AttackerWeaponMass = Mass * MassMod
       Force = Force * MassMod



endfunction



Function MomentumMessage()
Momentum = momentum - 1
                 If Momentum * 100 >= MessagePercent

                        Float Rounded = ((((Momentum * 100) * 100 + 0.5) / 100.0) As Int) As Float
                        String Out = StringUtil.SubString(Rounded As String, 0, StringUtil.Find(Rounded As String, ".", 0) + 2 )

                                            if hasMperk == 1
                               debug.Notification(out + "% " + "Momentum x2")
                            else
                               debug.Notification(out + "% " + "Momentum Bonus")
                            Endif

                    EndIf
            Momentum = 0
endfunction



Event OnItemAdded(Form akBaseItem, int aiItemCount, ObjectReference akItemReference, ObjectReference akSourceContainer)

if akBaseItem == aadpKnockBackHitToken
me.removeitem(aadpKnockBackHitToken, 1, true)

float TimeOut = 0

while Hitnow == 0 && Timeout < 0.5
utility.wait(0.05)
TimeOut = TimeOut + 0.05
endwhile

if Hitnow == 1
StaggerThemNow()
endif

HitNow = 0
endif

endEvent





State BusyState

endstate*/
}

