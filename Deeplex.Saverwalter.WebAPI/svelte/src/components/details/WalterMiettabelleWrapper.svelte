<script lang="ts">
    import type { WalterUmlageEntry, WalterVertragEntry } from "$walter/lib";
    import { Grid, Tab, TabContent, Tabs, Tile } from "carbon-components-svelte";
    import WalterMiettabelle from "./WalterMiettabelle.svelte";
    import WalterRechnungenTabelle from "./WalterRechnungenTabelle.svelte";

    export let vertraege: WalterVertragEntry[];
    export let umlagen: WalterUmlageEntry[];
    export let fetchImpl: typeof fetch;

    const mieten = vertraege.flatMap(vertrag => vertrag.mieten)
        .sort((a, b) => new Date(a.betreffenderMonat).getTime() - new Date(b.betreffenderMonat).getTime());
    const years: number[] = [];
    for(let i = new Date(mieten[0].betreffenderMonat).getFullYear();
        i < new Date(mieten[mieten.length - 1].betreffenderMonat).getFullYear() + 1;
        ++i)
    {
        years.push(i);
    }

    let selected = years.findIndex(year => year === new Date().getFullYear()) || years.length - 1;
</script>

<Tabs bind:selected type="container">
    {#each years as year}
        <Tab label={`${year}`}/>
    {/each}
    <svelte:fragment slot="content">
        <Tile>
        <div style="margin: 1em">
            <WalterMiettabelle
                year={years[selected]}
                {mieten}
                {vertraege} />
        </div>
        <div style="margin: 1em">
            <WalterRechnungenTabelle
                year={years[selected]}
                {fetchImpl}
                {umlagen} />
        </div>
        </Tile>
    </svelte:fragment>
</Tabs>