
<script lang="ts">
	import '@carbon/styles/css/styles.css'
	import '@carbon/charts-svelte/styles.css'
	import { LineChart } from '@carbon/charts-svelte'
    import type { WalterDataType } from './WalterData';

	export let title: string;
	export let data: WalterDataType;

	const options = {
		legend: { enabled: false },
		title: title,
		height: '400px',
		axes: {
			bottom: { mapsTo: 'key', scaleType: 'labels'  },
			left: { mapsTo: 'value', scaleType: 'labels' }
		},
        curve: 'curveMonotoneX'
	} as any; // TODO scaleType is not correct
</script>

<LineChart {data} {options} />
