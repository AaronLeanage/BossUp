# BossUp!

A PlateUp! mod that adds **boss customers**.

Every few days, a boss walks into your restaurant and orders **every dish on your menu**. Starters, mains, every side, desserts: the lot. Serve them everything before they run out of patience and they'll pay handsomely for it. Let them walk out and it's game over, just like any other customer.

## Features

- **Boss customers** that order every item your restaurant currently offers, in every course.
- **Single customer or a full boss table.** Face one very hungry customer, or a whole group sized to your current tables.
- **Configurable schedule.** Bosses can turn up every day or as rarely as every 15 days, and keep coming in overtime.
- **During the day or at closing time.** Choose whether the boss arrives mid-service or as the last group of the day.
- **Boss marker on the timeline.** Just like rushes, a marker on the day timeline shows when the boss is due, so you can prepare.
- **Crowned bosses.** Every boss wears a crown, so you can spot them in the queue and at the table.
- **Scaled patience.** Bosses get extra patience for each additional item they order, so a big menu is tough but fair.
- **Big rewards.** Every dish served to a boss pays a multiple of its normal price, shown as its own **Boss Bonus** line in the end-of-day summary.

## Settings

All settings are in the **BossUp!** preferences menu (main menu and pause menu). In multiplayer, only the host's settings are used and the menu is locked for everyone else.

| Setting | Options | Default |
|---|---|---|
| Bosses | Off / On | On |
| Boss frequency | Every day to every 15 days | Every 15 days |
| Arrival | During day / End of day | During day |
| Boss party | Single customer / Full group | Single customer |
| Orders | Menu items / Include variants | Menu items |
| Min orders per phase | Off / 3 / 5 / 8 | 5 |
| Max orders per phase | 4 / 8 / 12 / Unlimited | 8 |
| Patience scaling | Low / Medium / High | Medium |
| Bonus multiplier | 1x / 1.5x / 2x / 2.5x / 3x / 4x / 5x | 3x |
| Boss marker on timeline | Off / On | On |

**Orders:** *Menu items* means one of each dish on the menu. *Include variants* means one of each version of a dish (for example, each burger topping combination), so use **Max orders per phase** to keep it sane.

**Min orders per phase:** on a small menu, the boss repeats dishes until they've ordered at least this many per course (sides aren't padded). Max wins if it's lower than Min.

## Requirements

Subscribe to these on the Steam Workshop:

- [KitchenLib](https://steamcommunity.com/sharedfiles/filedetails/?id=2898069883)
- [HarmonyX](https://steamcommunity.com/sharedfiles/filedetails/?id=2898033283)
- [PreferenceSystem](https://steamcommunity.com/sharedfiles/filedetails/?id=2949018507)

## Manual installation

If you're not installing from the Workshop, put the DLL in PlateUp's `Mods` folder so the `content` folder sits directly inside the mod's folder:

```
PlateUp\Mods\BossUp\content\BossUp.dll
```

An extra folder level (for example `Mods\BossUp-1.0.0\BossUp\content\BossUp.dll`) stops the mod loading, and `Player.log` shows `no ModPacks found`.

## Multiplayer

Every player needs BossUp! installed. The host controls all boss settings; everyone else follows them automatically.

## Tips

- **Pick your moment.** Look for the crown: you can leave a seated boss waiting to order while you clear other tables first.
- **Bring plates.** A boss orders a lot of food at once, and BossUp! does not give you extra plates. Plan ahead if you're running bosses.
- Bosses don't appear in practice mode.

## Known limitations

- Some dishes with unusual ordering rules (such as tacos or cakes) may be left out of a boss's order.

## Ideas for v2

These are ideas I'd like to add in a future version. They haven't been checked for feasibility yet, so some may change or never arrive if they turn out not to be possible.

- **Boss types:**
  - *Critic:* half patience, double bonus.
  - *Glutton:* orders again after eating.
  - *Picky:* no sides, but every variant of the mains.
  - *Impatient:* patience drains faster as the day goes on.
  - *Messy:* leaves extra mess behind.
  - *VIP:* other customers wait while the boss eats.
- **Escalation:** each boss you beat makes the next one harder and more rewarding.
- **Reserved table:** an appliance that holds a table for the boss group.
- **HQ trophy:** a decoration in your HQ that tracks bosses beaten across runs.
- **Walkout options:** choose what happens when a boss walks out: *End game* (as now) or *Just lose bonus*, where the boss group leaves without paying but the game carries on.
- **Plate assist:** an option to receive extra plates on boss days.
- **Special dish support:** bring cakes, tacos and other special-case dishes into boss orders, one at a time.

## License

BossUp! is released under the [MIT License](LICENSE).
