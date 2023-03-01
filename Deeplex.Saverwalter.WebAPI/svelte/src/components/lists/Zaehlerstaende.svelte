<script lang="ts">
	import { WalterDataWrapper } from '$components';

	import type { ZaehlerstandEntry } from '$types';

	const headers = [
		{ key: 'datum', value: 'Datum' },
		{ key: 'stand', value: 'Stand' },
		{ key: 'einheit', value: 'Einheit' }
	];

	export let rows: Promise<ZaehlerstandEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;
</script>

<WalterDataWrapper {title} {search} {rows} {headers} />
