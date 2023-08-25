<script lang="ts">
    import { Content, Grid, Tab, TabContent, Tabs, Tile } from 'carbon-components-svelte';
    import { WalterHeader } from '$walter/components';
    import type { PageData } from './$types';
    import WalterDataHeatmapChart from '$walter/components/data/WalterDataHeatmapChart.svelte';
    import { walter_data_miettabelle } from '$walter/components/data/WalterData';

    export let data: PageData;
    const mieten = data.rows.flatMap(vertrag => vertrag.mieten)
        .sort((a, b) => new Date(a.betreffenderMonat).getTime() - new Date(b.betreffenderMonat).getTime());
    
    const years: number[] = [];
    for(let i = new Date(mieten[0].betreffenderMonat).getFullYear();
        i < new Date(mieten[mieten.length - 1].betreffenderMonat).getFullYear() + 1;
        ++i)
    {
        years.push(i);
    }
    
    const selected = years.findIndex(year => year === new Date().getFullYear()) || years.length - 1;
</script>

<WalterHeader title="SaverWalter" />
<Content>
    <div  style="text-align: center; margin-top: 5em">
        <Grid>
            <Tile>
                <h4>Miettabelle</h4>
                <Tabs {selected}>
                    {#each years as year}
                    <Tab label={`${year}`}/>
                    {/each}
                    <svelte:fragment slot="content">
                        {#each years as year}
                        <TabContent>
                            <WalterDataHeatmapChart config={walter_data_miettabelle(data.rows, year)} />
                        </TabContent>
                        {/each}
                    </svelte:fragment>
                </Tabs>
            </Tile>
        </Grid>
    </div>
</Content>
