### Stat Modifier Editor

The stat modifier editor is a tool that allows you to create and edit stat modifier.

A stat modifier is an effect that is applied to your stat.

The formula is:

`Stat value = (BaseValue + Sum(FlatModifier)) * (1 + Sum(PercentModifier) / 100) * Sum(MultiplierModifier)`

---

Here is a description of each input:

- Name: The name of the stat modifier, this is what will be shown to you inside the Stat Modifier Manager.
  <br/>
- Description: The description of the stat modifier (Optional).
  <br/>
- Select Stat: The stat assignated to this stat modifier. If you have a "strength" stat, and you want to create a modifier for this stat, then you need to put the "Strength" stat.
  <br/>
- Modifier Type: The type of this modifier, it can be either Flat, Percent, or Multiplier
  <br/>
  - Flat: It will be added (+) to the base value
    <br/>
  - Percent: It will be divised by 100 (so 20 = 20%), then multiplied (*) to the (BaseValue + Sum(FlatModifier))
  <br/> 
  - Multiplier: It will be multiplied (*) to the ((BaseValue + Sum(FlatModifier)) * (1 + Sum(PercentModifier))
  <br/>
- Stacking Type: What the modifier do if the same modifier is already applied to the character.
  <br/> 
  - Stack: Modifiers of the same type will stack.
    <br/>
  - Strongest: If the new modifier is stronger than the current, the new modifier will **replace** the current modifier.
    <br/>
  - Override: The new modifier will **replace** the current modifier.
    <br/>
  - Ignore: The new modifier will be ignored.
  <br/>
- Value: The value of the modifier. Note: The value CAN be edited in most situation, this is simply the default value to put if you add this modifier to, for example, an item. If you create a modifier 'More Strength I' with a value of 10, and add it to a weapon, you will **STILL be able to edit it** inside the item editor!
  <br/>
- Duration: The duration of the modifier, if you put "0" in all of them, this will be a forever. Else, if you put 10 seconds, then when the modifier is added to a character, it will be removed 10 seconds after.
