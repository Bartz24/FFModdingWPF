13-2 rando/AP next steps:

-- Try to fix fragment grant on crux some more, what is up with that
Field quest upper limit removal, is this just done in area scripts?
- just make quest locked items not need the quest anymore?
Review + fix any autodata generation stuff (such as mog levels)
-- Make changes to base rando for area union rather than intersection (Especially in oerba)
-- add more events/fakechecks for area progressions (such as dying world) to setup chains properly
pre-add all fragments to "pool" as fixed locations in prep for:
endgame logic adjustments, add fragment gating to final bosses? or alyssa?
- fragment hunt
- graviton hunt (fixed endgame)
- shop level hunt?
- ???

add more stuff to the actual randomised pool again woo
--clock puzzle count adjustment (min of override and normal value), allow timeouts?
dying woirld/bodhum 700 enemy rando fix
--prevent multiple fencers in encounters (special flag to prevent duplicates)
--deck caius being oerba caius gives infinite re-raise - ban oerba caius from caius locations - hack, prevent beach caius from moving for now

AP remove casino items from pool
AP UT area "spoiler" integration improvements - needs some client magic idk
AP item categorisation improvements + check group disabling?

--redo scaling factors for enemy rando, not quite ramping up enough currently?

correct fixed flags on fake check items if missing (checks should be enough for this to work?)

void beyond B paradox ending also unlock vanilla location in crux to remove softlock potential
look at gate script from zone table to figure it out probablty

giving out pre-existing shop levels breaks generation??

FIXED: clock puzzle timer behaviour adjusted
FIXED: fixed item location items getting added to pool


play around with the scripts to see if we can manipulate db contents somehow??

crystarium stuff:
can we just put items in the node list?
abilities as items?

make a fake battle scene for gorgyra/ugallu and do items in post script like other bosses
zenobia can be moved from acad 400

acad 500 fragemnt text crashes. lol.

Update artefacts to be copies of an artefact for categorisation/icon purposes

add twilight odin, long gui etc to boss token pool, investiaget scripts for ugallu/gorgyra, allow adjust threshold
- custom battle scenes to replace normal fights
- then clear the mission from the script instead?
- how easily can we setup custom fight hook scripts, is it just in the btsc table?

figure out why yomi/raspatil sometimes duplicate
mark several encounter slots as dangerous
only allow superbosses to be in dangerous spots
Add client log saying "hold L2/R2 to receive items" after the "you are now autotracking"


Casino stuff:
i16ScriptArg1 seems to be casino coin cost
u1OnlyOne seems to be shop duplicate check? Just category instead?
Casino tickets are 50/100/500
items currently are in the 10k range
logic for this is messy


AP Shop rando:
Add new shops for shop_ap_lxx in shop.wdb
Add new script flags for shops
Setup wdb with item slots (n per shop level?)
Setup check logic on ap side for it
Ensure they get set as once only buys (mess with this flag a bit)
Potentially also split up shop levels into different blocks?

Mess around with:
Adding new shops
playing with i16ScriptArg1 on casino stuff
u1OnlyOne to see if that is shop duplicate buy (and what happens if you have one already)
shop locations (sSignId) as well as content (can we cross over item pools cleanly?)


Augusta 300 scripts (zone 0093) still refer to entropy board a lot - should they?


Requirements for changes to add remaining fragments to the pool
- Fragment Missions
  - Currently unsure what options we have short of reconstructing the entire mission block in the game scripts
  - Ideally a way to change the reward a mission gives you would be ideal here, poke at the table and see what happens? Change things to point at fake items maybe which we can intercept?
- Fragment Fights
  - Might be easier to change
  - Mission table refers to a given encounter, might be able to replace the encounter entry with a different fight which we can add script hooks for on defeat similar to other boss callbacks for item grants (Zenobia etc)
- Captain Cryptic / Brain Blast
  - Not currently sure where this even happens...
- Blitz Squad
  - Can probably get in the pool at this point tbh
- Temporal Rifts in other non-oerba areas
  - Just needs a manual review, probably doable

Active changelog:
- Add Oerba Temporal Rift fragments to the pool
- Add Serendipity fragments to the pool
  - Casino coin fragments cost 50 coins total for now, and are logically blocked by getting at least one casino ticket
  - Currently Lucky Coin is marked as missable due to the amount not being easy to manipulate
  - This will likely be refined in the future
- Add missing fragment skill flags to the pool
- Remove Serendipity Wild Artefact treasure from the pool as it is unused by the game scripts/inaccessible
- Add indicator for pending item collection when running an AP seed
- Various check logic fixes
- Adjust "Old Device" to still be accessible as a check even when the Rhodium Ring sidequest is handed in (Bresha 100)