
enum EffectTypes {
    CORE_STATS, # (HP, MP, STR, DEF, AGI, INT, LUK)
    STATUS_EFFECTS, # (Poison, Burn, Freeze, Paralysis, Sleep, Confusion, Silence, Blind, Stun)
    COMBAT_EFFECTS, # (Damage, Heal, Crit, Accuracy, Evasion, Block, Reflect, Counter, LifeSteal, ManaSteal, ElementalDamage)
    ITEM_EFFECTS, # (Equip, Unequip, Consume, Drop, Inventory, Shop)
    SKILL_EFFECTS, # (Cast, Learn, Forget, SkillBoost, ReduceSkillCooldown)
    OTHER_EFFECTS, # (Experience, Money, Flag, Unflag, Quest, Message, Sound, Animation, Teleport, ChangeMap, Script)
}