<script lang="ts">
	import '@carbon/styles/css/styles.css'
	import '@carbon/charts-svelte/styles.css'
	import { PieChart } from '@carbon/charts-svelte'
    import type { WalterDataType } from './WalterData';

	export let title: string;
	export let data: WalterDataType;

	const options = {
		legend: { enabled: false },
		title: title,
		height: '400px',
	};
</script>

<PieChart {data} {options} />
