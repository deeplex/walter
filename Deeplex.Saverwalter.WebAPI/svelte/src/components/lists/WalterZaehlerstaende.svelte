<script lang="ts">
	import { WalterDataWrapper } from '$WalterComponents';

	import type { WalterZaehlerstandEntry } from '$WalterTypes';

	const headers = [
		{ key: 'datum', value: 'Datum' },
		{ key: 'stand', value: 'Stand' },
		{ key: 'einheit', value: 'Einheit' }
	];

	export let rows: Promise<WalterZaehlerstandEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;
</script>

<WalterDataWrapper {title} {search} {rows} {headers} />
