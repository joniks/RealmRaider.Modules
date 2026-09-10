using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace RealmRaiders.Modules.CharacterMotionProfiles.Tests
{
    public sealed class CharacterMotionPresentationResolverTests
    {
        [Test]
        public void Resolve_UsesExplicitPriorityAcrossMutuallyExclusiveStateDomains()
        {
            Assert.That(Resolve(MotionPresentationReaction.Death, MotionPresentationAttack.Ability, MotionPresentationJumpPhase.Landing, true), Is.EqualTo(MotionClipKey.Death));
            Assert.That(Resolve(MotionPresentationReaction.Hit, MotionPresentationAttack.Ability, MotionPresentationJumpPhase.Landing, true), Is.EqualTo(MotionClipKey.Hit));
            Assert.That(Resolve(MotionPresentationReaction.None, MotionPresentationAttack.Primary, MotionPresentationJumpPhase.Landing, true), Is.EqualTo(MotionClipKey.AttackPrimary));
            Assert.That(Resolve(MotionPresentationReaction.None, MotionPresentationAttack.Ability, MotionPresentationJumpPhase.Landing, true), Is.EqualTo(MotionClipKey.AttackAbility));
            Assert.That(Resolve(jumpPhase: MotionPresentationJumpPhase.Takeoff, isLocomoting: true), Is.EqualTo(MotionClipKey.JumpTakeoff));
            Assert.That(Resolve(jumpPhase: MotionPresentationJumpPhase.Falling, isLocomoting: true), Is.EqualTo(MotionClipKey.JumpFall));
            Assert.That(Resolve(jumpPhase: MotionPresentationJumpPhase.Landing, isLocomoting: true), Is.EqualTo(MotionClipKey.JumpLand));
            Assert.That(Resolve(isLocomoting: true), Is.EqualTo(MotionClipKey.Locomotion));
            Assert.That(Resolve(), Is.EqualTo(MotionClipKey.Idle));
        }

        [Test]
        public void Resolve_SanitizesUnknownEnumValuesToSafeLowerPriorityState()
        {
            Assert.That(Resolve((MotionPresentationReaction)99, MotionPresentationAttack.Ability, MotionPresentationJumpPhase.Landing, true), Is.EqualTo(MotionClipKey.AttackAbility));
            Assert.That(Resolve(MotionPresentationReaction.None, (MotionPresentationAttack)99, MotionPresentationJumpPhase.Landing, true), Is.EqualTo(MotionClipKey.JumpLand));
            Assert.That(Resolve(MotionPresentationReaction.None, MotionPresentationAttack.None, (MotionPresentationJumpPhase)99, true), Is.EqualTo(MotionClipKey.Locomotion));
            Assert.That(Resolve((MotionPresentationReaction)99, (MotionPresentationAttack)99, (MotionPresentationJumpPhase)99, false), Is.EqualTo(MotionClipKey.Idle));
        }

        [Test]
        public void Input_IsReadonlyValueTypeAndResolverHasNoUnityOrGameRuntimeDependency()
        {
            Assert.That(typeof(CharacterMotionPresentationInput).IsValueType, Is.True);
            Assert.That(typeof(CharacterMotionPresentationInput).GetProperties(BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite), Is.True);
            var dependencies = typeof(CharacterMotionPresentationResolver).Assembly.GetReferencedAssemblies().Select(assembly => assembly.Name).ToArray();
            Assert.That(dependencies.Any(name => name.StartsWith("UnityEngine")), Is.False);
            Assert.That(dependencies.Any(name => name == "RealmRaiders.Runtime"), Is.False);
        }

        private static MotionClipKey Resolve(
            MotionPresentationReaction reaction = MotionPresentationReaction.None,
            MotionPresentationAttack attack = MotionPresentationAttack.None,
            MotionPresentationJumpPhase jumpPhase = MotionPresentationJumpPhase.None,
            bool isLocomoting = false)
        {
            return CharacterMotionPresentationResolver.Resolve(new CharacterMotionPresentationInput(
                reaction,
                attack,
                jumpPhase,
                isLocomoting));
        }
    }
}
