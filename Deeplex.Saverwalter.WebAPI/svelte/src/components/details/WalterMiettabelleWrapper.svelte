<script lang="ts">
    import type { WalterMieteEntry, WalterVertragEntry } from "$walter/lib";
    import { Grid, Tab, TabContent, Tabs, Tile } from "carbon-components-svelte";
    import { convertDateCanadian } from "$walter/services/utils";
    import WalterDataWrapperQuickAdd from "../elements/WalterDataWrapperQuickAdd.svelte";
    import WalterMiete from "./WalterMiete.svelte";
    import WalterMiettabelle from "./WalterMiettabelle.svelte";

    export let vertraege: WalterVertragEntry[];

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

<div  style="margin-top: 5em">
    <Grid>
        <Tile>
            <h2>Miettabelle</h2>
            <Tabs bind:selected>
                {#each years as year}
                <Tab label={`${year}`}/>
                {/each}
                <svelte:fragment slot="content">
                    {#each years as year}
                    <TabContent>
                        <WalterMiettabelle
                            selectedYear={years[selected]}
                            {vertraege}
                            {year}/>
                    </TabContent>
                    {/each}
                </svelte:fragment>
            </Tabs>
        </Tile>
    </Grid>
</div>