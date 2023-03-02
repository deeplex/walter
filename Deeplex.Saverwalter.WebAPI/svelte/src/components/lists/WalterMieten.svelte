<script lang="ts">
	import { WalterDataWrapper } from '$WalterComponents';
	import type { WalterMieteEntry } from '$WalterTypes';

	const headers = [
		{ key: 'betreffenderMonat', value: 'Betreffender Monat' },
		{ key: 'zahlungsdatum', value: 'Zahlungsdatum' },
		{ key: 'betrag', value: 'Betrag' }
	];

	export let rows: Promise<WalterMieteEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;
</script>

<WalterDataWrapper {title} {search} {rows} {headers} />
