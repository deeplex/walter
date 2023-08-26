
<script lang="ts">
	import '@carbon/styles/css/styles.css'
	import '@carbon/charts-svelte/styles.css'
	import { HeatmapChart } from '@carbon/charts-svelte'
    import type { WalterDataConfigType } from './WalterData';
    import { onMount } from 'svelte';

	export let config: WalterDataConfigType;
	export let year: number;
	export let click: ((e: CustomEvent<any>, config: WalterDataConfigType, year: number) => void) | undefined = undefined;

	let heatMap : HeatmapChart;

	onMount(() => {
		if (click)
		{
			heatMap.$$.root.addEventListener('click', (e: CustomEvent<any>) => click!(e, config, year));
		}
	});

</script>

<HeatmapChart bind:this={heatMap} data={config.data} options={config.options} />
