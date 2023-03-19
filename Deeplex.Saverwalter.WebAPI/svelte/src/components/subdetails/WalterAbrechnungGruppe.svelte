<script lang="ts">
	import { goto } from '$app/navigation';
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

	const navigate = (e: any) => {
		const punkt = e.detail as WalterBetriebskostenabrechnungKostenpunkt;
		goto(
			`/betriebskostenrechnungen/${punkt.betriebskostenrechnungId || 'new'}`
		);
	};
</script>

<WalterDataTable {navigate} {headers} {rows} />
