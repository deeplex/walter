<script lang="ts">
	import { WalterDataWrapper } from '..';
	import type { MietminderungListEntry } from '../../types/mietminderunglist.type';
	const headers = [
		{ key: 'beginn', value: 'Beginn' },
		{ key: 'ende', value: 'Ende' },
		{ key: 'minderung', value: 'Minderung' }
	];

	// TODO
	// const navigate = (e: CustomEvent<DataTableRow>) =>
	// 	goto(`/rechnungen/betriebskosten/${e.detail.id}`);

	export let rows: Promise<MietminderungListEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;
</script>

<WalterDataWrapper {title} {search} {rows} {headers} />
