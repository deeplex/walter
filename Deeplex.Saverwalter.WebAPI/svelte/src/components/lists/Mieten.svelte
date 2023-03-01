<script lang="ts">
	import { WalterDataWrapper } from '$components';
	import type { MieteEntry } from '$types';

	const headers = [
		{ key: 'betreffenderMonat', value: 'Betreffender Monat' },
		{ key: 'zahlungsdatum', value: 'Zahlungsdatum' },
		{ key: 'betrag', value: 'Betrag' }
	];

	export let rows: Promise<MieteEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;
</script>

<WalterDataWrapper {title} {search} {rows} {headers} />
