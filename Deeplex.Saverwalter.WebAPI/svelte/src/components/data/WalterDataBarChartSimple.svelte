<script lang="ts">
    import '@carbon/styles/css/styles.css';
    import '@carbon/charts-svelte/styles.css';
    import {
        BarChartSimple,
        type BarChartOptions
    } from '@carbon/charts-svelte';
    import type { WalterDataType } from './WalterData';

    export let config: { data: WalterDataType; options: BarChartOptions };
</script>

<BarChartSimple data={config.data} options={config.options} />
