<script lang="ts">
	import { WalterDataTable } from '$WalterComponents';
	import type { WalterBetriebskostenabrechnungKostenpunkt } from '$WalterTypes';

	export let rows: WalterBetriebskostenabrechnungKostenpunkt[];

	const headers = [
		{ key: 'typ.text', value: 'Kostenanteil' },
		{ key: 'schluessel.text', value: 'Schlüssel' },
		{ key: 'nutzungsintervall', value: 'Nutzungsintervall' },
		{ key: 'betrag', value: 'Betrag' },
		{ key: 'anteil', value: 'Ihr Anteil' },
		{ key: 'kosten', value: 'Ihre Kosten' }
	];
</script>

<WalterDataTable {headers} {rows} />
