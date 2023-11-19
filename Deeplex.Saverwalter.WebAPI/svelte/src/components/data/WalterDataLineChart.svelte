<script lang="ts">
    import '@carbon/styles/css/styles.css';
    import '@carbon/charts-svelte/styles.css';
    import { LineChart } from '@carbon/charts-svelte';
    import type { WalterDataConfigType } from './WalterData';

    export let config: WalterDataConfigType;
</script>

<LineChart data={config.data} options={config.options} />
