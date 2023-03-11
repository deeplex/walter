<script lang="ts">
	import { WalterVertraege, WalterHeaderList } from '$WalterComponents';
	import type { PageData } from './$types';

	export let data: PageData;
</script>

<WalterHeaderList apiUrl={data.apiURL} title="Verträge" />
<WalterVertraege
	wohnungen={data.wohnungen}
	kontakte={data.kontakte}
	search
	rows={data.rows}
/>
