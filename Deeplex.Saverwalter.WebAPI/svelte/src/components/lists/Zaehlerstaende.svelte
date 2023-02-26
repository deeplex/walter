<script lang="ts">
	import { WalterDataWrapper } from '..';
	import type { ZaehlerstandEntry } from '../../types/zaehlerstandlist.type';

	const headers = [
		{ key: 'datum', value: 'Datum' },
		{ key: 'stand', value: 'Stand' }
	];

	export let rows: Promise<ZaehlerstandEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;
</script>

<WalterDataWrapper {title} {search} {rows} {headers} />
