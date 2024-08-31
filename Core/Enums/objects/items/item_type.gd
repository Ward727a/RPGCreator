
enum ItemType {
    WEAPON, # Items that can be equipped in the weapon slot (e.g. swords, bows, etc)
    ARMOR, # Items that can be equipped in the armor slot (e.g. helmets, chestplates, etc)
    CONSUMABLE, # Items that can be consumed to restore health, mana, etc (e.g. potions, food, etc)
    TRINKET, # Items that can be equipped in the trinket slot (e.g. rings, amulets, etc)
    QUEST, # Items that are used to complete quests (e.g. keys, quest items, etc)
    KEY, # Items that are used to unlock doors or chests (e.g. keys, lockpicks, etc)
    MISC, # Miscelaneous items (e.g. crafting materials, etc)
    COMODITY, # Items only intended to be sold (e.g. gold, gems, trash, etc)
    CUSTOM # Custom items type defined by the user
}