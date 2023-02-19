<script lang="ts">
	import { WalterDataWrapper } from '..';
	import type { ZaehlerstandListEntry } from '../../types/zaehlerstandlist.type';
	const headers = [
		{ key: 'datum', value: 'Datum' },
		{ key: 'stand', value: 'Stand' }
	];

	// TODO
	// const navigate = (e: CustomEvent<DataTableRow>) =>
	// 	goto(`/rechnungen/betriebskosten/${e.detail.id}`);

	export let rows: Promise<ZaehlerstandListEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;
</script>

<WalterDataWrapper {title} {search} {rows} {headers} />
