<script lang="ts">
	import { WalterDataWrapper } from '..';
	import type { MieteEntry } from '../../types/miete.type';

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
