<script lang="ts">
    import '@carbon/styles/css/styles.css';
    import '@carbon/charts-svelte/styles.css';
    import { HeatmapChart } from '@carbon/charts-svelte';
    import type { WalterDataConfigType } from './WalterData';
    import { onMount } from 'svelte';

    export let config: WalterDataConfigType;
    export let click:
        | ((e: CustomEvent<unknown>, config: WalterDataConfigType) => void)
        | undefined = undefined;

    let heatMap: HeatmapChart;

    onMount(() => {
        if (click) {
            heatMap.$$.root.addEventListener(
                'click',
                (e: CustomEvent<unknown>) => click!(e, config)
            );
        }
    });
</script>

<HeatmapChart
    bind:this={heatMap}
    bind:data={config.data}
    bind:options={config.options}
/>
