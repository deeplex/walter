
<script lang="ts">
	import '@carbon/styles/css/styles.css'
	import '@carbon/charts-svelte/styles.css'
	import { ScatterChart } from '@carbon/charts-svelte'
    import type { WalterDataConfigType } from './WalterData';

	export let config: WalterDataConfigType;
</script>

<ScatterChart data={config.data} options={config.options} />
