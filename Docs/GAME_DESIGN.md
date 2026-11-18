# Factory 2025 — Game Design

Unity project: `Factory_Project_2025` (Unity 6000.0.32, to be upgraded to Unity 6000.3.21f1).
Working title: _TBD_.

This project is a **demo / prototype** for a bigger game of the user's own, in the spirit of Satisfactory.

---

## 1. What kind of game this is

- A **realistic factory and business game**: you start your own company, design your own product, and build the factory that makes it.
- **No limits on what you make**: your own cereal brand, a machine workshop, a snack brand, and so on. You choose how it's made and what goes into it.
- Building feels like **Satisfactory** (place machines, connect them with belts); item handling feels like **Hydroneer** (physical items that fall off belts with physics).
- On top of the factory sits a **real business**: suppliers, trucks, staff, scientists, deals with other companies, and competitors.

---

## 2. Core loop

1. **Design a product**: choose what it is, its ingredients/components and how it's made.
2. **Source ingredients and parts**: order raw and special ingredients from suppliers; they arrive by truck.
3. **Build the production line**: machines for each step, connected by belts; physical items move through.
4. **Make the product**, broken down into **every individual component** (e.g. cereal flakes, coating, box, printed label, packed box).
5. **Store and ship**: inventory in storage; hire trucks to move it.
6. **Sell**: make deals with companies (retailers, distributors) for money; compete on price and quality.
7. **Reinvest**: upgrade machines, expand, hire staff, research better recipes.

---

## 3. Systems

### Product design (the "no limits" part)
- A **product is a recipe tree**: the final product is made of components, each made of ingredients or parts, each made by a machine step.
- Players choose ingredients, processes and packaging, which changes **cost, quality, taste/performance and appeal**.
- Product **categories** (food like cereal and snacks; mechanical parts; others) each have their own ingredient and machine sets, all built from the same data system so new categories can be added without new code.
- Branding: name, packaging look _(TBD how deep)_.

### Factory building (Satisfactory-style)
- Place machines on the factory floor; ghost preview shows valid/invalid placement.
- **Belts** dragged from one machine port to another as splines; auto-curving; mesh generated along the spline.
- Splitters, mergers, hoppers, storage.

### Item physics (Hydroneer-style)
- Items are **real physics objects** (Rigidbodies).
- Each physics step, a belt finds where an item sits along its spline and sets the item's along-belt speed to the belt speed; gravity still applies; a small centring force keeps items on the belt around curves.
- Items **fall off the end with real physics** and drop into machine hoppers through triggers.
- Performance: object pooling, sleeping items at rest, caps on loose items.

### Machines and upgrades
- Each machine performs a process step (mix, cook, cut, press, coat, package, assemble…).
- **Upgrade machines** to do more: speed, capacity, quality, new process options.

### Supply and logistics
- **Order raw and special ingredients** from suppliers (price, quality, delivery time).
- **Trucks**: order deliveries in; **hire trucks** to move your inventory out to buyers.
- Loading docks, storage and inventory management.

### Staff and research
- **Hire scientists** who give direction: suggest better ingredients, improve recipes, unlock new processes and products.
- _Other staff (workers, managers, drivers) — TBD._

### Business and competition
- **Deals with companies** for money: contracts, volumes, prices, deadlines.
- **Competitors** around you making similar products; market share, pricing and quality competition.
- Finances: money, costs, profit.

---

## 4. What already exists (prototype)
- Conveyor belt mesh generator between two rollers (A side / B side waypoints), with linear, Bézier and Catmull-Rom path options.
- A belt validator (distance, bend angle, ghost colours) — not yet wired up.
- Splines package installed but not used.
- Ground tile models, a conveyor belt model, ghost materials.

### Known bugs
- `BeltPathTracer.cs:45` — `GetComponent<MeshFilter>() ?? AddComponent` doesn't work with Unity's null handling.
- `BeltValidator.ValidateSetup()` is never called, so validation and ghost colours never update.
- The anchor points `WaypointManager` calculates are never used for the curve.
- Joining the A-side and B-side point lists makes the belt cross over itself.

---

## 5. Build order for the demo (suggested)
1. Upgrade the project.
2. Belt building with Splines (drag port to port, ghost preview).
3. Physical items on belts; falling off ends; machine hoppers.
4. A couple of machines and one simple product recipe (e.g. a cereal: grain → flakes → box → packed box).
5. Ingredient orders arriving by truck; storage; selling to one buyer for money.
6. Machine upgrades.
7. Then: product designer, scientists, competitors, deals, more categories.

---

## 6. Open questions
- Setting and era (modern day?), and whether there's a town/map outside the factory.
- First-person, third-person, or top-down building?
- How deep product design goes (choosing ingredients only, or designing packaging/branding too?).
- How competitors behave (AI companies with their own factories?).
- Workers: do staff physically run machines and drive trucks, or is it abstract?
- Is there a story or campaign, or sandbox only?
