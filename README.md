# One Pager for “Tour Tokyo”

## Overview:

This is a 2D route-planning game on a stylized Tokyo rail map. Your goal is to maximize Memory Points (MP) within one game day (time budget Z). The map is a graph of well-known locations connected by colored train lines (e.g., Green, Orange, Magenta). You may travel to any location on the same colored line; the game auto-sums segment time between your current station and the target. At intersections, you can either Transfer (no visit, pay a small T_transfer) or Visit (pay Y_visit, gain X MP; your next departure on a different line does not pay T_transfer). Visiting any location costs Y_visit time. Locations award X MP (values differ), and completing all locations in a genre (Cultural/Historical, Popular Culture, Art, Nature, Shopping, Landmark) yields a set bonus. Some locations offer opt-in events where you can spend a bit more time for a small bonus. You choose a starting station (or Random Start). Exact values for X / Y (segments) / Y_visit / T_transfer / Z will be tuned during playtests.

## Controls:

- [x] Select Destination: Hover a station to preview total time (segments + visit, and if applicable Transfer vs Visit totals at intersections) and MP; left-click to select.
- [x] Confirm Travel: Click Travel to execute the move and deduct time.
- [x] Intersection Choice: If the destination is an intersection, show Transfer (+T_transfer) and Visit (+Y_visit, +X MP; next transfer free).
- [x] Non-intersection: Show Visit only.
- [x] Tooltips: MP, genre tag(s), segment count, visit time, and transfer note.
  - Implemented as a info UI panel

## Art Assets:

- [x] Map & Lines: Minimal background; colored line segments; intersection nodes; station markers with hover/selected states.
- [x] Locations: ~12 simple icons (by genre) plus a small “event” badge.
- [x] HUD: Timer bar/number, MP total, genre/set-progress tracker, preview panel, basic buttons.
- [ ] FX: Lightweight travel sweep/arrow; MP pop numbers.
- [ ] Accessibility: Color-blind-safe palette for line colors; high-contrast icons.

## Audio Assets:

- [x] UI/Travel: click, confirm, travel whoosh, MP tick.
- [ ] Background: 1 short looping BGM suited to relaxed planning (optional late-run intensity lift).

## Game Flow:

- [x] Start at chosen (or random) station with full Z (one game day).
  - Start at Tokyo Station for Now
- [x] Select destination on the same colored line → preview total time (segments + visit; at intersections show Transfer vs Visit totals) and MP.
  - Shown on info board but not clear enough
- [x] Travel & arrive → choose Visit (pay Y_visit, gain X MP; optional event) or Transfer (no visit).
  - [ ] Optional event
- [x] Transfers: If you stopped at an intersection and your next move departs on a different line, add T_transfer—unless you Visited at that intersection (next transfer free).
- [x] Repeat until you’re out of time → Results (total MP, genres completed, visited list).

## Concerns:

- [x] Interesting Choices on a Small Graph: With ≈12 locations, place 2–3 longer segments and a few strategic intersections so transfer vs. distance trade-offs matter; ensure ≥3 viable genre routes within Z.
- [x] Preview Clarity: Keep tooltips succinct (MP, segments/transfer choice, visit time). Show both Transfer and Visit totals at intersections.
- [ ] Event Balance: Opt-in events should be small, readable bonuses; cap frequency so they never dominate route value.
- [x] Tuning Risk: X / Y / Y_visit / T_transfer / Z must yield typical runs of ~6–8 visits; adjust segment weights and transfer cost to avoid a single dominant path.
- [x] Line Colors & Readability: Use distinct hues + patterns for accessibility; avoid visual overlap where lines run in parallel.
