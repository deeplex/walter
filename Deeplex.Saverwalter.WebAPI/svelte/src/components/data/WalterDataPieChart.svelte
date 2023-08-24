<script lang="ts">
	import '@carbon/styles/css/styles.css'
	import '@carbon/charts-svelte/styles.css'
	import { PieChart } from '@carbon/charts-svelte'
    import type { WalterDataConfigType, WalterDataType } from './WalterData';

	export let config: WalterDataConfigType;
</script>

<PieChart data={config.data} options={config.options} />
