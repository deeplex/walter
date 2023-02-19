<script lang="ts">
	import { WalterDataWrapper } from '..';
	import type { MieteListEntry } from '../../types/mietelist.type';
	const headers = [
		{ key: 'betreffenderMonat', value: 'Betreffender Monat' },
		{ key: 'zahlungsDatum', value: 'Zahlungsdatum' },
		{ key: 'betrag', value: 'Betrag' }
	];

	// TODO
	// const navigate = (e: CustomEvent<DataTableRow>) =>
	// 	goto(`/rechnungen/betriebskosten/${e.detail.id}`);

	export let rows: Promise<MieteListEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;
</script>

<WalterDataWrapper {title} {search} {rows} {headers} />
