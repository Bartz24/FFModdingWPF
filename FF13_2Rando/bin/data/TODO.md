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